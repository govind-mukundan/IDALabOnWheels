using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// TODO: Calibration, IDENTIFICATION
// Inertial Measurement Unit - http://www.starlino.com/imu_guide.html

#region Notes

// Data format from the sensor board

/*
typedef struct {
  int16_t  accSmooth[3];
  int16_t  gyroData[3];
  int16_t  magADC[3];
  int16_t  gyroADC[3];
  int16_t  accADC[3];
} imu_t;

typedef struct {
  int32_t  EstAlt;             // in cm
  int16_t  vario;              // variometer in cm/s
  int32_t  Temp;
} alt_t;

typedef struct {
  int16_t angle[2];            // absolute angle inclination in multiple of 0.1 degree    180 deg = 1800
  int16_t heading;             // variometer in cm/s
} att_t;

 * Heading : Is the offset of the board from Magnetic North in degrees, varies from 0 to +/- 180
 * Pitch (X) : Is the rotation along the axis passing parallel through the Wings of an aircraft
 * Roll (Y) : Is the rotation along the axis passing through the head and tail of the aircraft
*/


#endregion

namespace IDALabOnWheels
{

    public class Acceleration
    {
        public float aX;
        public float aY;
        public float aZ;

        public Acceleration(float x, float y, float z)
        {
            this.aX = x;
            this.aY = y;
            this.aZ = z;
        }
    }

    public class Rotation
    {
        public float Yaw; // Head direction, rotation about this axis changes the direction a plane is heading
        public float Roll;
        public float Pitch;

        public Rotation(float x, float y, float z)
        {
            this.Yaw = x;
            this.Roll = y;
            this.Pitch = z;
        }
    }

    public class MagneticField
    {
        public float hX;
        public float hY;
        public float hZ;

        public MagneticField(float x, float y, float z)
        {
            this.hX = x;
            this.hY = y;
            this.hZ = z;
        }
    }

    // This represents the data created by puttint together the Gyro+Acc+Magneto data
    /// <summary>
    /// Attitude gives you the orientation of the object in space in degrees.
    /// X = PITCH
    /// Y = ROLL
    /// Head = YAW
    /// </summary>
    public class Attitude
    {
        public float angleX;
        public float angleY;
        public float heading;


        public Attitude()
        {
            this.angleX = 0f;
            this.angleY = 0f;
            this.heading = 0f;
        }

        public Attitude(float x, float y, float z)
        {
            this.angleX = x;
            this.angleY = y;
            this.heading = z;
        }
    }

    public class State
    {
        Attitude Attitude;
        float Altutude;
        float Temperature;
    }


    // Encapsulates the communication with the board
    class EWBSensorBoard
    {
        bool _debug = false;
        bool _simulate = false;

        string MSP_HEADER = "$M<";


        const byte MSP_IDENT = 100;
        const byte MSP_STATUS = 101;
        const byte MSP_RAW_IMU = 102;
        const byte MSP_ATTITUDE = 108;
        const byte MSP_ALTITUDE = 109;
        const byte MSP_ACC_CALIBRATION = 205;
        const byte MSP_MAG_CALIBRATION = 206;

        SerialPortAdapter _comInterface;

        // Queues for all the sensor data
        const int C_MAX_Q_SIZE = 1024; // No more than 1024 samples will be queued
        BlockingCollection<Acceleration> AccQ = new BlockingCollection<Acceleration>(new FixedSizeConcurrentQueue<Acceleration>(C_MAX_Q_SIZE));
        BlockingCollection<Rotation> GyroQ = new BlockingCollection<Rotation>(new FixedSizeConcurrentQueue<Rotation>(C_MAX_Q_SIZE));
        BlockingCollection<MagneticField> MagnetoQ = new BlockingCollection<MagneticField>(new FixedSizeConcurrentQueue<MagneticField>(C_MAX_Q_SIZE));
        BlockingCollection<Attitude> AttitudeQ = new BlockingCollection<Attitude>(new FixedSizeConcurrentQueue<Attitude>(C_MAX_Q_SIZE));
        BlockingCollection<float> AltitudeQ = new BlockingCollection<float>(new FixedSizeConcurrentQueue<float>(C_MAX_Q_SIZE));
        BlockingCollection<float> TemperatureQ = new BlockingCollection<float>(new FixedSizeConcurrentQueue<float>(C_MAX_Q_SIZE));

        // It roughly takes 5 - 6 mS from a Tx request to get the Raw data, so the sample interval should be slower than that
        const int C_SAMPLE_INTERVAL_RAW_MS = 20;
        const int C_SAMPLE_INTERVAL_ATT_MS = 25;
        const int C_SAMPLE_INTERVAL_ALT_MS = 100;
        const int C_SAMPLE_INTERVAL_STAT_MS = 200;
        // Example commands: IDENT - Tx 0x244d3c006464, Rx - 24 4d 3e 01 64 0a 6f
        public Attitude CurrentAttitude; // memorry of the last attitide

        State _currentState;

        public void Test()
        {
            // byte[] data = GenerateRequest(MSP_IDENT, new byte[0]);
            //RequestIMUData();

        }

        public void Start()
        {
            _currentState = new State();
            _stopCommunication = false;
            Task task = Task.Factory.StartNew(() => CommThread());
        }

        public void Stop()
        {
            _stopCommunication = true;
        }


        public EWBSensorBoard()
        {
            _comInterface = new SerialPortAdapter();
            _comInterface.SerialDataRxedHandler = RxByteStreamParser;
        }

        public bool Open(string comPort)
        {
            Debug.WriteLine("Opening port to:" + comPort);
            return (_comInterface.Open(comPort));
        }

        public void DoOnQuit()
        {
            if (_comInterface != null)
            {
                _comInterface.Close();
            }
            _stopCommunication = true;
        }
        public void RequestIMUData()
        {
            GenericRequest(MSP_RAW_IMU);
        }

        public void RequestTemperature()
        {

        }

        public void RequestStatus()
        {
            GenericRequest(MSP_STATUS);
        }

        public void RequestAltitude()
        {
            GenericRequest(MSP_ALTITUDE);
        }

        public void RequestAttitude()
        {
            GenericRequest(MSP_ATTITUDE);
        }

        public void RequestAccCalib()
        {
            GenericRequest(MSP_ACC_CALIBRATION);
        }

        public void RequestMagCalib()
        {
            GenericRequest(MSP_MAG_CALIBRATION);
        }

        public Attitude[] GetAttitude()
        {
            return (AttitudeQ.GetAvailableData());
        }

        float _simXIncrement = .5f;
        Attitude _simAttitude = new Attitude();
        Attitude SimulateAttitude()
        {
            //_simAttitude.angleY += _simXIncrement;
            _simAttitude.angleY += _simXIncrement/1;

            return (_simAttitude);
        }

        public Attitude GetAverageAttitude()
        {
            if (_simulate) return (SimulateAttitude()); // Simulation

            Attitude[] att = null;
            Attitude avg = new Attitude();

            att = GetAttitude();
            if (att == null) return (null);

            float[][] angles = new float[3][];
            angles[0] = new float[att.Length]; angles[1] = new float[att.Length]; angles[2] = new float[att.Length];
            for (int i = 0; i < att.Length; i++)
            {
                angles[0][i] = att[i].angleX;
                angles[1][i] = att[i].angleY;
                angles[2][i] = att[i].heading;
            }

            avg.angleX = Utility.Median(angles[0]);
            avg.angleY = Utility.Median(angles[1]);
            avg.heading = Utility.Median(angles[2]);

            CurrentAttitude = avg;
            return (avg);
        }

        public MagneticField[] GetCompass()
        {
            return (MagnetoQ.GetAvailableData());
        }
        public Rotation[] GetGyro()
        {
            return (GyroQ.GetAvailableData());
        }
        public Acceleration[] GetAcceleration()
        {
            return (AccQ.GetAvailableData());
        }

        public float[] GetAltitude()
        {
            return (AltitudeQ.GetAvailableData());
        }

        public float[] GetTemperature()
        {
            return (TemperatureQ.GetAvailableData());
        }


        private void GenericRequest(byte cmd)
        {
            lock (this)
            {
                _waitingForResponse = true;
            }
            _comInterface.WriteData(GenerateRequest(cmd, new byte[0]));
        }

        /// <summary>
        /// Given a byte command and a payload, returns a packet to send to the board
        /// <header>,<direction>,<size>,<command>,<data>,<crc>
        /// cmd can range from UINT8 to UINT32, but here we assume only byte commands. Size only counts the data length (not command)
        /// All data is little endian
        /// Reference: http://www.multiwii.com/wiki/index.php?title=Multiwii_Serial_Protocol
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public byte[] GenerateRequest(byte cmd, byte[] payload)
        {
            byte[] request = new byte[MSP_HEADER.Length + payload.Length + 1 + 1 + 1]; // header + payload + cmd +size + crc
            int index = 0;
            byte crc = 0;

            // 1. Header + direction
            for (int i = 0; i < MSP_HEADER.Length; i++)
            {
                request[index++] = (byte)MSP_HEADER.ToCharArray()[i];
            }

            // 2. Size
            request[index++] = (byte)payload.Length;
            crc ^= (byte)payload.Length;

            // 3. Command
            request[index++] = cmd;
            crc ^= cmd;

            // 4. Data
            for (int i = 0; i < payload.Length; i++)
            {
                request[index++] = payload[i];
                crc ^= payload[i];
            }

            // 5. CRC
            request[index++] = crc;

            return (request);
        }


        enum ParserState
        {
            Idle,
            LookForHeader,
            ParsePayload,
            Error
        }
        ParserState _parserState = ParserState.LookForHeader;
        /// <summary>
        /// Takes in a stream of bytes and converts it into sensor data points
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns> Number of bytes successfully converted into valid data. If no bytes were read, 0 is returned.</returns>
        public int RxByteStreamParser(byte[] bytes)
        {
            int consumed_bytes = 0;

            while (consumed_bytes < bytes.Length)
            {

                switch (_parserState)
                {
                    case ParserState.LookForHeader:
                        // Header has at least 3 bytes
                        if (bytes.Length < 3) return consumed_bytes;

                        if (bytes[0] == '$' && bytes[1] == 'M' && bytes[2] == '>')
                        {
                            _parserState = ParserState.ParsePayload; // Move to next state
                            consumed_bytes += 3;

                            //Debug.WriteLine("------------------------RX-Header-----------------------------");
                            //for (int i = 0; i < bytes.Length; i += 1)
                            //    Debug.Write(Convert.ToString(((bytes[i]))) + " ");
                            //Debug.WriteLine("\r\n-----------------------------------------------------");

                            if (bytes.Length > 3)
                            {
                                Array.Copy(bytes, 3, bytes, 0, bytes.Length - 3);
                                Array.Resize(ref bytes, bytes.Length - 3);
                            }
                        }
                        else if (bytes[0] == '$' && bytes[1] == 'M' && bytes[2] == '!')
                        {
                            consumed_bytes += 3;
                            throw (new Exception("ERROR Message Received From Sensor Board!!"));
                        }
                        else
                        {
                            consumed_bytes += 1; // Discard one byte
                            if (bytes.Length > 1)
                            {
                                Array.Copy(bytes, 3, bytes, 0, bytes.Length - 1);
                                Array.Resize(ref bytes, bytes.Length - 3);

                            }
                            Debug.Write("Unexpected Byte!\n");
                        }
                        break;

                    case ParserState.ParsePayload: //<size>,<command>,<data>,<crc>
                        byte payload_size = bytes[0];
                        int expected_len = payload_size + 1 + 1 + 1;
                        // Wait till we get all the remaining bytes = cmd + payload + crc
                        if (bytes.Length < expected_len) return consumed_bytes;

                        // Now we have all the bytes, evaluate it and clear the buffer
                        // Check CS
                        if (!VerifyCS(bytes, bytes[payload_size + 1 + 1]))
                        {
                            throw (new Exception("Checksum Error in Message Received From Sensor Board!!")); return 0;
                        }
                        Debug.WriteLineIf(_debug,"------------------------RX-Payload-----------------------------");
                        Debug.WriteLineIf(_debug, DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture));
                        for (int i = 0; i < bytes.Length; i++)
                            Debug.WriteIf(_debug, Convert.ToString(((bytes[i]))) + " ");
                        Debug.WriteLineIf(_debug, "\r\n-----------------------------------------------------");
                        // Evaluate response
                        switch (bytes[1])
                        {
                            case MSP_IDENT:
                                Debug.WriteLineIf(_debug, "MSP_IDENT");
                                break;

                            case MSP_STATUS:
                                Debug.WriteLineIf(_debug, "MSP_STATUS");
                                break;
                            case MSP_RAW_IMU:
                                AccQ.Add(new Acceleration(Int16ToF(bytes[2], bytes[3]), Int16ToF(bytes[4], bytes[5]), Int16ToF(bytes[6], bytes[7])));
                                GyroQ.Add(new Rotation(Int16ToF(bytes[2], bytes[3]) / 8, Int16ToF(bytes[4], bytes[5]) / 8, Int16ToF(bytes[6], bytes[7]) / 8));
                                MagnetoQ.Add(new MagneticField(Int16ToF(bytes[2], bytes[3]) / 3, Int16ToF(bytes[4], bytes[5]) / 3, Int16ToF(bytes[6], bytes[7]) / 3));
                                Debug.WriteLineIf(_debug, "MSP_RAW_IMU");
                                break;
                            case MSP_ATTITUDE:
                                AttitudeQ.Add(new Attitude(Int16ToF(bytes[2], bytes[3]) / 10, Int16ToF(bytes[4], bytes[5]) / 10, Int16ToF(bytes[6], bytes[7])));
                                Debug.WriteLineIf(_debug, "MSP_ATTITUDE");
                                break;
                            case MSP_ALTITUDE:
                                AltitudeQ.Add(Int32ToF(bytes[2], bytes[3],bytes[4],bytes[5]));
                                TemperatureQ.Add(Int32ToF(bytes[8], bytes[9], bytes[10], bytes[11]) / 100);
                                Debug.WriteLineIf(_debug, "MSP_ALTITUDE");
                                break;
                            case MSP_ACC_CALIBRATION:
                                Debug.WriteLineIf(_debug, "MSP_ACC_CALIBRATION");
                                break;
                            case MSP_MAG_CALIBRATION:
                                Debug.WriteLineIf(_debug, "MSP_MAG_CALIBRATION");
                                break;

                        }

                        consumed_bytes += expected_len;
                        _parserState = ParserState.LookForHeader; // Move to next state
                        lock (this)
                        {
                            _waitingForResponse = false; // Got the response already!
                        }

                        if (bytes.Length > expected_len)
                        {
                            Array.Copy(bytes, expected_len, bytes, 0, bytes.Length - expected_len);
                            Array.Resize(ref bytes, bytes.Length - expected_len);
                        }
                        break;

                }
            }

            return (consumed_bytes);
        }

        float Int16ToF(byte lsb, byte msb)
        {
            return (float) ((Int16)(((Int16)msb << 8) | lsb));
        }

        float Int32ToF(byte lsb, byte mlsb, byte mmsb, byte msb)
        {
            return (float) ((Int32)(((Int32)msb << 24) | ((Int32)mmsb << 16) | ((Int32)mlsb << 8) | lsb));
        }


        bool VerifyCS(byte[] data, byte cs)
        {
            return (true);
        }

        bool _stopCommunication;
        bool _waitingForResponse;
        long[] _timeStamps;
        Stopwatch _stopWatch;

        void CommThread()
        {
            Debug.WriteLine("Starting Comm thread to EWB Sensor Board");
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            _timeStamps = new long[1];
            for (int i = 0; i < _timeStamps.Length; i++)
            {
                _timeStamps[i] = _stopWatch.ElapsedMilliseconds;
            }

            long timeCount = 0;

            while (!_stopCommunication)
            {
                if (_waitingForResponse)
                {
                    // handle timeout
                }
                else
                {
                    // We make sure that the raw data is sampled with least jitter
                    long now = _stopWatch.ElapsedMilliseconds;

                    if (now - _timeStamps[0] > C_SAMPLE_INTERVAL_RAW_MS)
                    {
                        timeCount++;

                        if (timeCount % 10 == 0)
                        {
                            RequestStatus();
                        }
                        else if (timeCount % 4 == 0)
                        {
                            RequestAltitude();
                        }
                        else if (timeCount % 2 == 0)
                        {
                            RequestAttitude();
                        }
                        else
                        {
                            RequestIMUData();
                        }
                    }
                }
            }


        }

    }
}

using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{

    public class TexContainer
    {
        public Texture Tex;
        public int ID;

        public TexContainer(Texture tex, int id)
        {
            this.Tex = tex;
            this.ID = id;
        }

    }

    /// <summary>
    /// Manages all the textures used in the scene.
    /// 
    /// 
    ///  You can load as many Textures as you want into the OpenGL memory, all assigned to Texture Unit #0. OpenGL always returns a new handle for each of these textures.
    ///  When applying the texture, you just have to bind to the appropriate handle via glBindTexture() and set the sampler to 0
    ///  For most cases you don't need to use the other texture units, it's only really required when you want to blend more than one texture on the fly.
    /// </summary>
    class TextureManager
    {


        Dictionary<string, TexContainer> _textureCollection;
        int _idCount;

        private static readonly TextureManager instance = new TextureManager();

        private TextureManager()
        {
            _textureCollection = new Dictionary<string, TexContainer>();
            _idCount = 0;
        }

        public static TextureManager Instance
        {
            get
            {
                if (instance == null)
                {
                    return instance;
                }
                return instance;
            }
        }

        //public TextureManager()
        //{
        //    _textureCollection = new Dictionary<string, TexContainer>();
        //    _idCount = 0;
        //}

        // http://www.mastropaolo.com/devildotnet/
        // Path is to the image (only png/jpg for now)
        // This creates a texture and assigns it to a texture unit on the GPU.
        // What happens if all the texture units are taken ??
        public void CreateTexture(string path, OpenGL gl, bool UseNewTU = false)
        {
            if (!_textureCollection.ContainsKey(path))
            {
                Debug.WriteLine("Found New Texture! " + "ID: " + _idCount.ToString() + " Path: " + path);

                if (UseNewTU)
                    _idCount++;

                int[] test = new int[3];
                gl.GetInteger(OpenGL.GL_MAX_TEXTURE_IMAGE_UNITS, test);
                if (test[0] <= _idCount)
                {
                    throw new Exception("Out of Texture Units");
                }

                Bitmap img = new Bitmap(path);
                TexContainer texC = new TexContainer(new Texture(), _idCount);

                gl.Enable(OpenGL.GL_TEXTURE_2D);
                gl.ActiveTexture(OpenGL.GL_TEXTURE0 + (uint)_idCount);
                texC.Tex.Create(gl, img);
                texC.Tex.Bind(gl);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
                _textureCollection.Add(path, texC);


            }
        }

        public Texture GetTexture(string path, OpenGL gl)
        {
            if (!_textureCollection.ContainsKey(path))
            {
                CreateTexture(path, gl);
            }
            return (_textureCollection[path].Tex);
        }

        public uint GetID(string path)
        {
            return ((uint)_textureCollection[path].ID);
        }

        public TexContainer GetElement(string path)
        {
            return (_textureCollection[path]);
        }

    }
}

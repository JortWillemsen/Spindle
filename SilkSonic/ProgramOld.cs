using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Drawing;
using StbImageSharp;

namespace SilkSonic;

public class ProgramOld
{
	private static IWindow _window;
	private static GL _gl;
	private static uint _vao;
	private static uint _vbo;
	private static uint _ebo;
	private static uint _program;
	private static uint _texture;

	public static void MainOld(string[] args)
	{
		WindowOptions options = WindowOptions.Default with
		{
			Size = new Vector2D<int>(800, 600),
			Title = "SilkSonic, now in concert nearby"
		};
		_window = Window.Create(options);
		_window.Load += OnLoad;
		_window.Update += OnUpdate;
		_window.Render += OnRender;
		_window.Run();
	}

	private static void OnLoad()
	{
		// Listen to input
		IInputContext input = _window.CreateInput();
		foreach (IKeyboard keyboard in input.Keyboards)
			keyboard.KeyDown += KeyDown;

		// DEFINING DATA

		_gl = _window.CreateOpenGL(); // Store reference to our OpenGL API instance
		_gl.ClearColor(Color.CornflowerBlue); // Define the clear color
		_gl.Clear(ClearBufferMask.ColorBufferBit); // Now actually clear the screen

		// Prepare a Vertex Array Object and bind it as a Vertex Array (prepare for use)
		_vao = _gl.GenVertexArray();
		_gl.BindVertexArray(_vao);

		// Prepare Vertex Buffer Object
		Span<float> vertices = stackalloc float[]
		// 4 vertices of a quad filling the screen, in the expected order (clockwise), but vertically flipped (OpenGL says 0,0 is on the bottom left instead of top left).
		{
			//    aPosition    | aTexCoords
			1.0f,  -1.0f, 0.0f,  1.0f, 1.0f,
			1.0f,   1.0f, 0.0f,  1.0f, 0.0f,
			-1.0f,  1.0f, 0.0f,  0.0f, 0.0f,
			-1.0f, -1.0f, 0.0f,  0.0f, 1.0f
		};
		_vbo = _gl.GenBuffer();
		_gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo); // Bind as an Array Buffer (VBO?)
		_gl.BufferData<float>(
			BufferTargetARB.ArrayBuffer,
			(nuint) (vertices.Length * sizeof(float)), // Length of the array
			vertices,                                       // The buffer to read from
			BufferUsageARB.StaticDraw);                // How we plan to use it, optimizing GPU

		// Optional step: Element Buffer Objects:
		// Allow deduplicating VAOs by giving EBOs filled with indices.
		// Basically, make up all triangles based on all points, where the indices
		// indicate what triangle belongs to what points. This way, a point can
		// be reused for multiple triangles: less data to store and process!
		Span<uint> indices = stackalloc uint[]
		{
			0u, 1u, 3u,
			1u, 2u, 3u
		};
		_ebo = _gl.GenBuffer();
		_gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo); // Bind as an EBO?

		_gl.BufferData<uint>(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), indices, BufferUsageARB.StaticDraw);


		// DEFINING SHADERS
		// The data defined above, now loaded into the buffers, must be processed by the GPU.
		// How, is defined in shaders. The two most common shaders are vertex shaders and fragment shaders.
		// Vertex shaders process the data representing vertices, to manipulate said vertices. This can scale, translate
		// or rate objects. A good example is defining a height map. After a vertex shader is complete, all vertices are
		// processed into triangles.
		// Now the fragment shader processes what every pixel should display, based on said triangles. Here, texturing
		// can be done.
		const string vertexCode = @"
#version 330 core

layout (location = 0) in vec3 aPosition;
// Add a new input attribute for the texture coordinates
layout (location = 1) in vec2 aTextureCoord;

// Add an output variable to pass the texture coordinate to the fragment shader
// This variable stores the data that we want to be received by the fragment
out vec2 frag_texCoords;

void main()
{
    gl_Position = vec4(aPosition, 1.0);
    // Assign the texture coordinates without any modification to be received in the fragment
    frag_texCoords = aTextureCoord;
}";

		const string fragmentCode = @"
#version 330 core

// Receive the input from the vertex shader in an attribute
in vec2 frag_texCoords;
uniform sampler2D uTexture;

out vec4 out_color;

void main()
{
    // This will allow us to see the texture coordinates in action!
	// out_color = vec4(frag_texCoords.x, frag_texCoords.y, 0, 1.0);
    out_color = texture(uTexture, frag_texCoords);
}";

		// Note that the shaders form a pipeline. Any custom `in`s can only be defined in the vertex shader.
		// VAOs can be filled with any data you want, and thus other `in`s may make sense.
		// More on this later.

		// Create shader as vertex shader
		uint vertexShader = _gl.CreateShader(ShaderType.VertexShader); // Create a shader in memory
		_gl.ShaderSource(vertexShader, vertexCode); // Set the source code of the shader

		// Compile
		_gl.CompileShader(vertexShader);
		_gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
		if (vStatus != (int) GLEnum.True)
			throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));

		// Create shader as fragment shader
		uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
		_gl.ShaderSource(fragmentShader, fragmentCode);

		// Compile
		_gl.CompileShader(fragmentShader);
		_gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
		if (fStatus != (int) GLEnum.True)
			throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));


		// Now that we compiled both shaders and have pointers to them, we must create a shader program.
		// We link the code of all shaders into a final program.
		_program = _gl.CreateProgram();
		_gl.AttachShader(_program, vertexShader);
		_gl.AttachShader(_program, fragmentShader);

		_gl.LinkProgram(_program);

		_gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
		if (lStatus != (int) GLEnum.True)
			throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));

		// Now that we have linked the program, we can free a bit of GPU memory again.
		// This removes the individual shader programs again and deletes them.
		_gl.DetachShader(_program, vertexShader);
		_gl.DetachShader(_program, fragmentShader);
		_gl.DeleteShader(vertexShader);
		_gl.DeleteShader(fragmentShader);



		// SETTING ATTRIBUTES - GIVING DATA TO THE SHADERS
		// Now we have set up our shader, but data needs to be fed into it.
		// Since VAOs can contain much information, we first need to tell OpenGL _what_ information it contains, and
		// most importantly: in what structure.
		// After we have defined the VAO's structure, we can feed it into the shader program.
		// If you defined any custom data in the VAOs, the custom `in`s in the vertex shader can read them now.

		// Define VAO structure: starting from 0 we contain Vector3's taking up 3 floats, which we do not want to normalize.
		// We increase the stride with 2 to make 5, so that the texture coordinates can be included as well. Note that these are not included as vertices, since they're not part of the attributes defined here.
		const uint positionLoc = 0; // Same value as the `aPosition` `in` in the vertex shader. We now ensure the data is
									// in the right place, as expected by the shader. Note: the position can be dynamic by using _gl.GetAttribLocation("aPosition");
		_gl.EnableVertexAttribArray(positionLoc);
		unsafe
		{
			_gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*) 0);
		}

		// Define texture coordinates structure
		const uint texCoordLoc = 1; // 1 after the first value (the vertex coordinates)
		_gl.EnableVertexAttribArray(texCoordLoc);
		unsafe
		{
			_gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float))); // Last value is actually the index from where we start to read
		}

		// Bind the texture
		_texture = _gl.GenTexture();
		_gl.ActiveTexture(TextureUnit.Texture0);
		_gl.BindTexture(TextureTarget.Texture2D, _texture);

		// Define pixels
		const int width = 256;
		const int height = 256;
		Span<byte> pixelData = stackalloc byte[width * 3 * height]; // 3 bytes per pixel
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				pixelData[x * 3 + y * width * 3] = (byte)Math.Abs(y - 128);
			}
		}
		_gl.TexImage2D<byte>(TextureTarget.Texture2D, 0, InternalFormat.Rgb, width,
			height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, pixelData);

		_gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
		_gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
		_gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.Nearest);
		_gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Nearest);

		int location = _gl.GetUniformLocation(_program, "uTexture"); // We use this method instead of a hardcoded 0 or 1 like we did earlier when defining layouts, because this time we chose not to define layouts and let GLSL figure things out.
		_gl.Uniform1(location, 0); // We bind texture unit 0 (defined earlier) to the uniform we found in the line above. Result: shader gets texture data



		// CLEAN UP (reduces risks of changing wrong buffer, but not always required)
		// Unbind everything
		_gl.BindVertexArray(0); // Must be cleaned first
		_gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
		_gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
		_gl.BindTexture(TextureTarget.Texture2D, 0);
	}

	private static void OnUpdate(double deltaTime) { }

	private static void OnRender(double deltaTime)
	{
		// We rebind some things which we cleaned up earlier...
		_gl.BindVertexArray(_vao);
		_gl.UseProgram(_program);
		_gl.ActiveTexture(TextureUnit.Texture0);
		_gl.BindTexture(TextureTarget.Texture2D, _texture);
		unsafe
		{
			_gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*) 0); // Draws an EBO
		}
		// _gl.DrawArrays(PrimitiveType.Triangles, 0, 6); // Draws a VBO
	}

	#region EventHandlers

	private static void KeyDown(IKeyboard keyboard, Key key, int keyCode) // TODO: Will be triggered even if window has no focus
	{
		if (key == Key.Escape) _window.Close();
	}

	#endregion
}
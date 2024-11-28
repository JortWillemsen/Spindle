using Engine.Cameras;
using Engine.Renderers;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Renderer.Display;

// ReSharper disable once InconsistentNaming
public class OpenGLDisplay : IDisplay
{
	/// <inheritdoc />
	public IRenderer Renderer { get; set; }

	/// <inheritdoc />
	public CameraManager CameraManager { get; set; }

	private IWindow _window;

	private GL _gl;
	private uint _vao;
	private uint _vbo;
	private uint _ebo;
	private uint _shaderProgram;
	private uint _textureId;

	private bool _hasFocus;

	public OpenGLDisplay(IRenderer renderer, Camera camera, int width, int height)
	{
		Renderer = renderer;
		CameraManager = new CameraManager(camera); // TODO: cameraManager

		WindowOptions options = WindowOptions.Default with
		{
			Size = new Vector2D<int>(width, height),
			Title = "Spindle, ask for parental advice before usage"
		};
		_window = Window.Create(options);
		_window.Load += OnLoad;
		_window.Render += OnRender;
		_window.Resize += OnResize;
		_window.FocusChanged += hasFocus => _hasFocus = hasFocus;
	}

	/// <inheritdoc />
	public void Show(params string[] args) => _window.Run(); // Blocking call


	private void KeyDown(IKeyboard keyboard, Key key, int keyCode)
	{
		if (!_hasFocus) return; // Only process any input when focussed

		switch (key)
		{
			case Key.Escape:
				_window.Close();
				break;
		}
	}

	private void OnLoad()
	{
		// Listen to input
		IInputContext input = _window.CreateInput();
		foreach (IKeyboard keyboard in input.Keyboards)
			keyboard.KeyDown += KeyDown;

		_gl = _window.CreateOpenGL(); // Store reference to our OpenGL API instance

		// _gl.ClearColor(Color.CornflowerBlue); // Define the clear color
		// _gl.Clear(ClearBufferMask.ColorBufferBit); // Now actually clear the screen

		// Prepare a Vertex Array Object and bind it as a Vertex Array (prepare for use)
		_vao = _gl.GenVertexArray();
		_gl.BindVertexArray(_vao);

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
		_shaderProgram = _gl.CreateProgram();
		_gl.AttachShader(_shaderProgram, vertexShader);
		_gl.AttachShader(_shaderProgram, fragmentShader);

		_gl.LinkProgram(_shaderProgram);

		_gl.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out int lStatus);
		if (lStatus != (int) GLEnum.True)
			throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_shaderProgram));

		// Now that we have linked the program, we can free a bit of GPU memory again.
		// This removes the individual shader programs again and deletes them.
		_gl.DetachShader(_shaderProgram, vertexShader);
		_gl.DetachShader(_shaderProgram, fragmentShader);
		_gl.DeleteShader(vertexShader);
		_gl.DeleteShader(fragmentShader);
	}

	private void OnRender(double deltaTime)
	{
		foreach (var camera in CameraManager.GetDisplayedCameras())
		{
			// DEFINING DATA
			var texture = new Texture(camera.DisplayRegion.Width, camera.DisplayRegion.Height, camera.DisplayRegion);
			Span<int> pi = texture.GetWritablePixels(camera.DisplayRegion.Width, camera.DisplayRegion.Height);
			camera.RenderShot(Renderer, pi);

			// Prepare Vertex Buffer Object
			Span<float> vertices = stackalloc float[] // todo: calculate these dynamically
			// 4 vertices of a quad filling the screen, in the expected order (clockwise), but vertically flipped (OpenGL says 0,0 is on the bottom left instead of top left).
			{
				//    aPosition    | aTexCoords
				1.0f,  -1.0f, 0.0f,  1.0f, 1.0f,
				1.0f,   1.0f, 0.0f,  1.0f, 0.0f,
				-1.0f,  1.0f, 0.0f,  0.0f, 0.0f,
				-1.0f, -1.0f, 0.0f,  0.0f, 1.0f
			};
			_vbo = _gl.GenBuffer(); // todo: voor later: deze vbos hergebruiken?
			_gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo); // Bind as an Array Buffer (VBO?)
			_gl.BufferData<float>(
				BufferTargetARB.ArrayBuffer,
				(nuint) (vertices.Length * sizeof(float)), // Length of the array
				vertices,                                  // The buffer to read from
				BufferUsageARB.DynamicDraw);               // How we plan to use it, optimizing GPU

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

			_gl.BufferData<uint>(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), indices, BufferUsageARB.DynamicDraw);



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



			// CREATE AND FILL TEXTURE

		    _textureId = _gl.GenTexture();
			_gl.ActiveTexture(TextureUnit.Texture0);
			_gl.BindTexture(TextureTarget.Texture2D, _textureId);

			// Fill the texture 'slot'
			_gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint) texture.Width,
				(uint) texture.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, texture.ReadPixels()); // todo: these now inclde transparancy, but we will never use that. Switch to bytes

			// Define attributes to how the texture should be rendered
			// _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
			// _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
			_gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.Linear);
			_gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Linear);

			// Assign the current texture to the texture uniform, such that the vertex shader gets this texture as input
			int location = _gl.GetUniformLocation(_shaderProgram, "uTexture"); // We use this method instead of a hardcoded 0 or 1 like we did earlier when defining layouts, because this time we chose not to define layouts and let GLSL figure things out.
			_gl.Uniform1(location, 0); // We bind texture unit 0 (defined earlier) to the uniform we found in the line above. Result: shader gets texture data




			// CLEAN UP (reduces risks of changing wrong buffer, but not always required) todo: remove this at the end
			// Unbind everything
			_gl.BindVertexArray(0); // Must be cleaned first
			_gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
			_gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
			_gl.BindTexture(TextureTarget.Texture2D, 0);

			// We rebind some things which we cleaned up earlier...
			_gl.BindVertexArray(_vao);
			_gl.UseProgram(_shaderProgram);
			_gl.ActiveTexture(TextureUnit.Texture0);
			_gl.BindTexture(TextureTarget.Texture2D, _textureId);
			unsafe
			{
				_gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*) 0); // Draws an EBO
			}
			// _gl.DrawArrays(PrimitiveType.Triangles, 0, 6); // Draws a VBO
			// TODO: SwapBuffers might be nice?
		}
	}

	private void OnResize(Vector2D<int> newSize)
	{
		throw new NotImplementedException();
		// _gl.Viewport( 0, 0, (uint) newSize.X, (uint) newSize.Y );
		CameraManager.SetDisplaySize(newSize);
	}
}
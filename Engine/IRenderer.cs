namespace Engine;

public interface IRenderer
{
    public Scene Scene { get; private set; }

    public void TraceRay(Ray ray, out int pixel)
    {
        
    }
}
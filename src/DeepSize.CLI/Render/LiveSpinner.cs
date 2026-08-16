using Spectre.Console;
using Spectre.Console.Rendering;

namespace DeepSize.CLI.Render;

public class LiveSpinner : Renderable
{
    private readonly Spinner _spinner;
    private readonly Style _style;
    private readonly IRenderable _label;
    
    private int _frameIndex = 0;

    public LiveSpinner(Spinner spinner, Style style, IRenderable label)
    {
        _spinner = spinner;
        _style = style;
        _label = label;
    }

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        if(_frameIndex >= _spinner.Frames.Count)
            _frameIndex = 0;
        
        var frame = _spinner.Frames[_frameIndex];
        var segments = new List<Segment>();
        IRenderable frameRenderable = new Text(frame, _style);
        
        
        segments.AddRange(frameRenderable.Render(options, maxWidth));
        segments.Add(Segment.Padding(1));
        segments.AddRange(_label.Render(options, maxWidth));

        _frameIndex++;
        
        return segments;
    }
}
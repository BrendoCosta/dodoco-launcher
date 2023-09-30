using System.Drawing;

namespace Dodoco.Application {

    public interface IApplicationWindow {

        bool IsFrameless();
        void SetFrameless(bool frameless);
        bool IsOpen();
        bool IsResizable();
        void SetResizable(bool resizable);
        Point GetPosition();
        void SetPosition(Point position);
        Size GetSize();
        void SetSize(Size size);
        string GetTitle();
        void SetTitle(string title);
        Uri GetUri();
        void SetUri(Uri uri);

        event EventHandler<bool> OnIsFramelessChanged;
        event EventHandler<bool> OnIsResizableChanged;
        event EventHandler<Point> OnPositionChanged;
        event EventHandler<Size> OnSizeChanged;
        event EventHandler<string> OnTitleChanged;
        event EventHandler<Uri> OnUriChanged;

        event EventHandler OnOpening;
        event EventHandler OnOpen;
        event EventHandler OnClosing;
        event EventHandler OnClose;
        event EventHandler<IApplicationWindow> OnChildWindowAdded;

        void Open();
        void Close();

        void AddChildWindow(IApplicationWindow child);
        object? GetRawObject();

    }
    
}
using PhotinoNET;
using System.Drawing;

namespace Dodoco.Application {

    public class ApplicationWindow: IApplicationWindow {

        private PhotinoWindow? windowObject = null;
        private List<PhotinoWindow?> childs = new List<PhotinoWindow?>();
        private Thread windowThread;

        private bool frameless;
        public bool open { get; private set; } = false;
        private bool resizable;
        private Point position;
        private Size size;
        private string title;
        private Uri uri;

        public bool IsFrameless() => this.frameless;
        public void SetFrameless(bool frameless) {
            this.frameless = frameless;
            this.OnIsFramelessChanged(this, frameless);
        }
        
        public bool IsOpen() => this.open;
        public void SetIsOpen(bool open) {
            this.open = open;
            this.OnIsResizableChanged(this, open);
        }
        
        public bool IsResizable() => this.resizable;
        public void SetResizable(bool resizable) {
            this.resizable = resizable;
            this.OnIsResizableChanged(this, resizable);
        }

        public Point GetPosition() => this.position;
        public void SetPosition(Point position) {
            this.position = position;
            this.OnPositionChanged(this, position);
        }

        public Size GetSize() => this.size;
        public void SetSize(Size size) {
            this.size = size;
            this.OnSizeChanged(this, size);
        }
        
        public string GetTitle() => this.title;
        public void SetTitle(string title) {
            this.title = title;
            this.OnTitleChanged(this, title);
        }
        
        public Uri GetUri() => this.uri;
        public void SetUri(Uri uri) {
            this.uri = uri;
            this.OnUriChanged(this, uri);
        }

        public event EventHandler OnOpening =
            new EventHandler((object? sender, EventArgs e) => {});
        
        public event EventHandler OnOpen =
            new EventHandler((object? sender, EventArgs e) => {});
        
        public event EventHandler OnClosing =
            new EventHandler((object? sender, EventArgs e) => {});
        
        public event EventHandler OnClose =
            new EventHandler((object? sender, EventArgs e) => {});
        
        public event EventHandler<bool> OnIsFramelessChanged =
            new EventHandler<bool>((object? sender, bool e) => {});
        
        public event EventHandler<Point> OnPositionChanged =
            new EventHandler<Point>((object? sender, Point e) => {});
        
        public event EventHandler<bool> OnIsResizableChanged =
            new EventHandler<bool>((object? sender, bool e) => {});
        
        public event EventHandler<Size> OnSizeChanged =
            new EventHandler<Size>((object? sender, Size e) => {});
        
        public event EventHandler<string> OnTitleChanged =
            new EventHandler<string>((object? sender, string e) => {});
        
        public event EventHandler<Uri> OnUriChanged =
            new EventHandler<Uri>((object? sender, Uri e) => {});
        
        public event EventHandler<IApplicationWindow> OnChildWindowAdded =
            new EventHandler<IApplicationWindow>((object? sender, IApplicationWindow e) => {});

        public ApplicationWindow() {

            // Default settings

            this.frameless = false;
            this.resizable = true;
            this.position = new Point(256, 256);
            this.size = new Size(720, 480);
            this.title = "Application Window";
            this.uri = new Uri("about:blank");

            this.windowThread = new Thread(() => {
                
                this.windowObject = new PhotinoWindow();

                this.windowObject.SetLogVerbosity(0);
                this.windowObject.SetUseOsDefaultLocation(true);
                this.windowObject.SetUseOsDefaultSize(false);
                this.windowObject.SetChromeless(this.IsFrameless());
                this.windowObject.SetResizable(this.IsResizable());
                this.windowObject.SetLocation(this.GetPosition());
                this.windowObject.SetSize(this.GetSize());
                this.windowObject.SetTitle(this.GetTitle());
                this.windowObject.Load(this.GetUri());

                this.OnIsFramelessChanged += new EventHandler<bool>((object? sender, bool e) => this.windowObject.SetChromeless(e));
                this.OnIsResizableChanged += new EventHandler<bool>((object? sender, bool e) => this.windowObject.SetResizable(e));
                this.OnPositionChanged    += new EventHandler<Point>((object? sender, Point e) => this.windowObject.SetLocation(e));
                this.OnSizeChanged        += new EventHandler<Size>((object? sender, Size e) => this.windowObject.SetSize(e));
                this.OnTitleChanged       += new EventHandler<string>((object? sender, string e) => this.windowObject.SetTitle(e));
                this.OnUriChanged         += new EventHandler<Uri>((object? sender, Uri e) => this.windowObject.Load(e));
                
                this.OnOpening            += new EventHandler((object? sender, EventArgs e) => this.windowObject.WindowCreatingHandler += this.OnOpening);
                this.OnOpen               += new EventHandler((object? sender, EventArgs e) => this.windowObject.WindowCreatedHandler += this.OnOpen);
                
                this.OnClose              += new EventHandler((object? sender, EventArgs e) => {
                    
                    foreach (PhotinoWindow? childWindow in this.childs) {

                        try { childWindow?.Close(); }
                        catch (System.ApplicationException) {}

                    }

                    this.windowObject.Close();
                    
                });

                this.OnChildWindowAdded   += new EventHandler<IApplicationWindow>((object? sender, IApplicationWindow e) => {

                    if (this.windowObject != null) {

                        PhotinoWindow? raw = (PhotinoWindow?) e.GetRawObject();
                        this.childs.Add(raw);
                        raw?.WaitForClose();

                    }

                });

                // Aditional configs

                this.windowObject.RegisterWindowClosingHandler(new PhotinoWindow.NetClosingDelegate((object sender, EventArgs e) => {
                    this.Close();
                    return false;
                }));

                // Manages child windows

                for (int i = 0; i < 10; i++) {

                    this.childs.Add(new PhotinoWindow(this.windowObject));

                }

                // Run

                this.windowObject.WaitForClose();

            });

            this.windowThread.TrySetApartmentState(ApartmentState.STA);

        }

        public void Open() {

            if (!this.IsOpen()) {

                this.OnOpening.Invoke(this, EventArgs.Empty);

                this.OnOpening.Invoke(this, EventArgs.Empty);
                this.windowThread.Start();
                this.SetIsOpen(true);
                this.OnOpen.Invoke(this, EventArgs.Empty);

            }

        }

        public void Close() {

            if (this.IsOpen()) {

                this.SetIsOpen(false);
                this.OnClosing.Invoke(this, EventArgs.Empty);
                this.OnClose.Invoke(this, EventArgs.Empty);
                this.windowThread.Join();

            }

        }

        public void AddChildWindow(IApplicationWindow child) {

            this.OnChildWindowAdded.Invoke(this, child);

        }

        public object? GetRawObject() {

            return this.windowObject;

        }

    }

}
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace T.Windows
{
    [CLSCompliant(true)]
    [ToolboxItem(false)]
    public class Popup : ToolStripDropDown
    {
        private bool focusOnOpen = true;
        private bool acceptAlt = true;
        public DateTime LastClosedTimeStamp = DateTime.Now;
        private IContainer components = (IContainer)null;
        private Control content;
        private bool fade;
        private Popup ownerPopup;
        private Popup childPopup;
        private bool _resizable;
        private bool resizable;
        private ToolStripControlHost host;
        private Size minSize;
        private Size maxSize;
        private const int frames = 1;
        private const int totalduration = 0;
        private const int frameduration = 0;
        private bool resizableTop;
        private bool resizableRight;
        private VisualStyleRenderer sizeGripRenderer;

        public Control Content
        {
            get
            {
                return this.content;
            }
        }

        public bool UseFadeEffect
        {
            get
            {
                return this.fade;
            }
            set
            {
                if (this.fade == value)
                    return;
                this.fade = value;
            }
        }

        public bool FocusOnOpen
        {
            get
            {
                return this.focusOnOpen;
            }
            set
            {
                this.focusOnOpen = value;
            }
        }

        public bool AcceptAlt
        {
            get
            {
                return this.acceptAlt;
            }
            set
            {
                this.acceptAlt = value;
            }
        }

        public bool Resizable
        {
            get
            {
                return this.resizable && this._resizable;
            }
            set
            {
                this.resizable = value;
            }
        }

        public new Size MinimumSize
        {
            get
            {
                return this.minSize;
            }
            set
            {
                this.minSize = value;
            }
        }

        public new Size MaximumSize
        {
            get
            {
                return this.maxSize;
            }
            set
            {
                this.maxSize = value;
            }
        }

        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 134217728;
                return createParams;
            }
        }

        public Popup(Control content)
        {
            Popup popup = this;
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            this.content = content;
            this.fade = SystemInformation.IsMenuAnimationEnabled && SystemInformation.IsMenuFadeEnabled;
            this._resizable = true;
            this.InitializeComponent();
            this.AutoSize = false;
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.host = new ToolStripControlHost(content);
            this.Padding = this.Margin = this.host.Padding = this.host.Margin = Padding.Empty;
            this.MinimumSize = content.MinimumSize;
            content.MinimumSize = content.Size;
            this.MaximumSize = content.MaximumSize;
            content.MaximumSize = content.Size;
            this.Size = content.Size;
            content.Location = Point.Empty;
            this.Items.Add((ToolStripItem)this.host);
            content.Disposed += (EventHandler)((sender, e) =>
            {
                content = (Control)null;
                popup.Dispose(true);
            });
            content.RegionChanged += (EventHandler)((sender, e) => this.UpdateRegion());
            content.Paint += (PaintEventHandler)((sender, e) => this.PaintSizeGrip(e));
            this.UpdateRegion();
            this.BackColor = Color.Black;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (this.acceptAlt && (keyData & Keys.Alt) == Keys.Alt)
                return false;
            return base.ProcessDialogKey(keyData);
        }

        protected void UpdateRegion()
        {
            if (this.Region != null)
            {
                this.Region.Dispose();
                this.Region = (Region)null;
            }
            if (this.content.Region == null)
                return;
            this.Region = this.content.Region.Clone();
        }

        public void Show(Control control)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));
            this.SetOwnerItem(control);
            this.Show(control, control.ClientRectangle);
        }
        
        public void Show(Control control, Rectangle area)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            SetOwnerItem(control);
            resizableTop = resizableRight = false;
            Point location = control.PointToScreen(new Point(area.Left, area.Top + area.Height));
            Rectangle screen = Screen.FromControl(control).WorkingArea;
            if (location.X + Size.Width > (screen.Left + screen.Width))
            {
                resizableRight = true;
                location.X = (screen.Left + screen.Width) - Size.Width;
            }
            if (location.Y + Size.Height > (screen.Top + screen.Height))
            {
                resizableTop = true;
                location.Y -= Size.Height + area.Height;
            }
            location = control.PointToClient(location);
            Show(control, location, ToolStripDropDownDirection.BelowRight);
        }

        protected override void SetVisibleCore(bool visible)
        {
            double opacity = this.Opacity;
            if (visible && this.fade && this.focusOnOpen)
                this.Opacity = 0.0;
            base.SetVisibleCore(visible);
            if (!visible || !this.fade || !this.focusOnOpen)
                return;
            for (int index = 1; index <= 1; ++index)
            {
                if (index > 1)
                    Thread.Sleep(0);
                this.Opacity = opacity * (double)index / 1.0;
            }
            this.Opacity = opacity;
        }

        private void SetOwnerItem(Control control)
        {
            if (control == null)
                return;
            if (control is Popup)
            {
                Popup popup = control as Popup;
                this.ownerPopup = popup;
                this.ownerPopup.childPopup = this;
                this.OwnerItem = popup.Items[0];
            }
            else
            {
                if (control.Parent == null)
                    return;
                this.SetOwnerItem(control.Parent);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.content.MinimumSize = this.Size;
            this.content.MaximumSize = this.Size;
            this.content.Size = this.Size;
            this.content.Location = Point.Empty;
            base.OnSizeChanged(e);
        }

        protected override void OnOpening(CancelEventArgs e)
        {
            if (this.content.IsDisposed || this.content.Disposing)
            {
                e.Cancel = true;
            }
            else
            {
                this.UpdateRegion();
                base.OnOpening(e);
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            if (this.ownerPopup != null)
                this.ownerPopup._resizable = false;
            if (this.focusOnOpen)
            {
                this.Activate(this.content);
                this.Activate((Control)this);
            }
            base.OnOpened(e);
        }

        private void Activate(Control c)
        {
            if (c.Parent != null)
                this.Activate(c.Parent);
            c.Focus();
            c.Select();
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            if (this.ownerPopup != null)
                this.ownerPopup._resizable = true;
            base.OnClosed(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (!this.Visible)
                this.LastClosedTimeStamp = DateTime.Now;
            base.OnVisibleChanged(e);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (this.InternalProcessResizing(ref m, false))
                return;
            base.WndProc(ref m);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public bool ProcessResizing(ref Message m)
        {
            return this.InternalProcessResizing(ref m, true);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private bool InternalProcessResizing(ref Message m, bool contentControl)
        {
            if (m.Msg == 134 && m.WParam != IntPtr.Zero && this.childPopup != null && this.childPopup.Visible)
                this.childPopup.Hide();
            if (!this.Resizable)
                return false;
            if (m.Msg == 132)
                return this.OnNcHitTest(ref m, contentControl);
            if (m.Msg == 36)
                return this.OnGetMinMaxInfo(ref m);
            return false;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private bool OnGetMinMaxInfo(ref Message m)
        {
            NativeMethods.MINMAXINFO structure = (NativeMethods.MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.MINMAXINFO));
            structure.maxTrackSize = this.MaximumSize;
            structure.minTrackSize = this.MinimumSize;
            Marshal.StructureToPtr((object)structure, m.LParam, false);
            return true;
        }

        private bool OnNcHitTest(ref Message m, bool contentControl)
        {
            Point client = this.PointToClient(new Point(NativeMethods.LOWORD(m.LParam), NativeMethods.HIWORD(m.LParam)));
            GripBounds gripBounds = new GripBounds(contentControl ? this.content.ClientRectangle : this.ClientRectangle);
            IntPtr num = new IntPtr(-1);
            if (this.resizableTop)
            {
                if (this.resizableRight && gripBounds.TopLeft.Contains(client))
                {
                    m.Result = contentControl ? num : (IntPtr)13;
                    return true;
                }
                if (!this.resizableRight && gripBounds.TopRight.Contains(client))
                {
                    m.Result = contentControl ? num : (IntPtr)14;
                    return true;
                }
                if (gripBounds.Top.Contains(client))
                {
                    m.Result = contentControl ? num : (IntPtr)12;
                    return true;
                }
            }
            else
            {
                if (this.resizableRight && gripBounds.BottomLeft.Contains(client))
                {
                    m.Result = contentControl ? num : (IntPtr)16;
                    return true;
                }
                if (!this.resizableRight && gripBounds.BottomRight.Contains(client))
                {
                    m.Result = contentControl ? num : (IntPtr)17;
                    return true;
                }
                if (gripBounds.Bottom.Contains(client))
                {
                    m.Result = contentControl ? num : (IntPtr)15;
                    return true;
                }
            }
            if (this.resizableRight && gripBounds.Left.Contains(client))
            {
                m.Result = contentControl ? num : (IntPtr)10;
                return true;
            }
            if (this.resizableRight || !gripBounds.Right.Contains(client))
                return false;
            m.Result = contentControl ? num : (IntPtr)11;
            return true;
        }

        public void PaintSizeGrip(PaintEventArgs e)
        {
            if (e == null || e.Graphics == null || !this.resizable)
                return;
            Size clientSize = this.content.ClientSize;
            if (Application.RenderWithVisualStyles)
            {
                if (this.sizeGripRenderer == null)
                    this.sizeGripRenderer = new VisualStyleRenderer(VisualStyleElement.Status.Gripper.Normal);
                this.sizeGripRenderer.DrawBackground((IDeviceContext)e.Graphics, new Rectangle(clientSize.Width - 16, clientSize.Height - 16, 16, 16));
            }
            else
                ControlPaint.DrawSizeGrip(e.Graphics, this.content.BackColor, clientSize.Width - 16, clientSize.Height - 16, 16, 16);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                    this.components.Dispose();
                if (this.content != null)
                {
                    Control content = this.content;
                    this.content = (Control)null;
                    content.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = (IContainer)new Container();
        }
    }
}

using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;

namespace T.Windows
{
    [ToolboxBitmap(typeof(ComboBox))]
    [ToolboxItem(true)]
    [ToolboxItemFilter("System.Windows.Forms")]
    [Description("Displays an editable text box with a drop-down list of permitted values.")]
    public class PopupComboBox : ComboBox
    {
        private IContainer components = (IContainer)null;
        protected Popup dropDown;
        private Control dropDownControl;

        public bool PopUpClosed { get; set; }

        public PopupComboBox()
        {
            this.InitializeComponent();
            base.DropDownHeight = base.DropDownWidth = 1;
            base.IntegralHeight = false;
        }

        private void IndexChanged(object sender, ToolStripDropDownClosingEventArgs e)
        {
            this.PopUpClosed = true;
            this.OnSelectedIndexChanged((EventArgs)e);
            this.PopUpClosed = false;
        }

        public Control DropDownControl
        {
            get
            {
                return this.dropDownControl;
            }
            set
            {
                if (this.dropDownControl == value)
                    return;
                this.dropDownControl = value;
                this.dropDown = new Popup(value);
                this.dropDown.BackColor = Color.Black;
                this.dropDown.Closing += new ToolStripDropDownClosingEventHandler(this.IndexChanged);
                this.dropDown.Width = 1;
                this.dropDown.FocusOnOpen = true;
            }
        }

        public void ShowDropDown()
        {
            if (this.dropDown == null)
                return;
            this.dropDown.Show((Control)this);
        }

        public void HideDropDown()
        {
            if (this.dropDown == null)
                return;
            this.dropDown.Hide();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 8465 && NativeMethods.HIWORD(m.WParam) == 7)
            {
                if (DateTime.Now.Subtract(this.dropDown.LastClosedTimeStamp).TotalMilliseconds <= 500.0)
                    return;
                this.ShowDropDown();
            }
            else
                base.WndProc(ref m);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new int DropDownWidth
        {
            get
            {
                return base.DropDownWidth;
            }
            set
            {
                base.DropDownWidth = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new int DropDownHeight
        {
            get
            {
                return base.DropDownHeight;
            }
            set
            {
                this.dropDown.Height = value;
                base.DropDownHeight = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool IntegralHeight
        {
            get
            {
                return base.IntegralHeight;
            }
            set
            {
                base.IntegralHeight = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new ComboBox.ObjectCollection Items
        {
            get
            {
                return base.Items;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new int ItemHeight
        {
            get
            {
                return base.ItemHeight;
            }
            set
            {
                base.ItemHeight = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                    this.components.Dispose();
                if (this.dropDown != null)
                    this.dropDown.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}

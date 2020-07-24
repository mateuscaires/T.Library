
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using T.Common;

namespace T.Windows
{
    public class TDataGridView : DataGridView
    {
        public const string CT_TXT_Filter = "Arquivo de texto|*.txt|Todos (*.*)|*";
        public const string CT_CSV_Filter = "Arquivo csv|*.csv|Todos (*.*)|*";
        public const string CT_XLSX_Filter = "Arquivo xlsx|*.xlsx|Todos (*.*)|*";
        public const string CT_PDF_Filter = "Arquivo pdf|*.pdf|Todos (*.*)|*";

        public new object DataSource { get { return base.DataSource; } set { if (ClearColumnsOnDataSourceChaged) Columns.Clear(); base.DataSource = value; } }
        public bool ClearColumnsOnDataSourceChaged { get; set; }
        public bool ChangeBackColor { get; set; }
        public new bool RowHeadersVisible { get { return base.RowHeadersVisible; } set { } }

        private DataGridViewCellStyle _cellStyle1;
        private DataGridViewCellStyle _cellStyle2;
        private DataGridViewCellStyle _cellStyle3;
        private DataGridViewCellStyle _cellStyle4;

        private Color _parentBackColor;
        private Color _defaultColor;
        private Color _gridColor;
        private bool _initilizedColor;
        private SaveFileDialog _saveFileDialog;

        private Color _parentForeColor;

        private Color _selectionBackColor;

        private Color _backColor
        {
            get
            {
                if (ChangeBackColor)
                    return BackgroundColor;

                if (_parentBackColor != Color.Empty)
                    return _parentBackColor;

                if (Parent != null)
                {
                    Control parent = Parent;

                    while (parent != null)
                    {
                        if (parent is Form)
                        {
                            //_parentForeColor = parent.ForeColor;
                            _parentBackColor = parent.BackColor;
                            return _parentBackColor;
                        }

                        parent = parent.Parent;
                    }
                }

                return _defaultColor;
            }
        }

        private Color _foreColor
        {
            get
            {
                if (_parentForeColor != Color.Empty)
                    return _parentForeColor;

                if (Parent != null)
                {
                    Control parent = Parent;

                    while (parent != null)
                    {
                        if (parent is Form)
                        {
                            _parentForeColor = parent.ForeColor;
                            _parentBackColor = parent.BackColor;
                            _selectionBackColor = _parentForeColor.GetTransparence();
                            return _parentBackColor;
                        }

                        parent = parent.Parent;
                    }
                }

                return _defaultColor;
            }
        }
        
        public TDataGridView()
        {
            ColumnHeadersVisible = false;
            ChangeBackColor = false;
            AllowUserToResizeRows = false;
            _parentBackColor = Color.Empty;
            _defaultColor = Color.Black;
            _parentForeColor = Color.Empty;
            _gridColor = Color.Empty;
            _selectionBackColor = Color.Empty;

            Initialize();

            SetMenu();
        }

        public void ExportExcel()
        {
            DialogResult resutl = ShowFileDialog(FileType.XLSX);

            if (resutl != DialogResult.OK)
                return;

            Tools.Excel.Instance.DataTableToExcel(GetDataTable(), _saveFileDialog.FileName);
            
            try
            {
                FileInfo fi = new FileInfo(_saveFileDialog.FileName);
                Process.Start("explorer.exe", fi.DirectoryName);
            }
            catch
            {

            }
        }

        public DataTable GetDataTable()
        {
            DataTable table = new DataTable();

            Dictionary<string, string> cols = new Dictionary<string, string>();

            foreach (DataGridViewColumn c in Columns)
            {
                cols.Add(c.Name, c.HeaderText);
                table.Columns.Add(c.HeaderText);
            }

            DataRow row;

            foreach (DataGridViewRow r in Rows)
            {
                row = table.NewRow();

                foreach (KeyValuePair<string, string> c in cols)
                {
                    row[c.Value] = r.Cells[c.Key].Value;
                }

                table.Rows.Add(row);

                table.AcceptChanges();
            }

            return table;
        }
        
        public DialogResult ShowFileDialog(FileType fileType)
        {
            return ShowFileDialog(fileType, string.Empty);
        }

        public DialogResult ShowFileDialog(FileType fileType, string fileName)
        {
            switch (fileType)
            {
                case FileType.TXT:
                    return ShowFileDialog(CT_TXT_Filter, fileName);
                case FileType.CSV:
                    return ShowFileDialog(CT_CSV_Filter, fileName);
                case FileType.XLSX:
                    return ShowFileDialog(CT_XLSX_Filter, fileName);
                case FileType.PDF:
                    return ShowFileDialog(CT_PDF_Filter, fileName);
                default:
                    return DialogResult.Abort;
            }
        }

        public DialogResult ShowFileDialog(string filter, string fileName)
        {
            DialogResult result = 0;

            Action invoker = () =>
            {
                _saveFileDialog = new SaveFileDialog();
                _saveFileDialog.FileName = fileName;
                _saveFileDialog.Filter = filter;
                result = _saveFileDialog.ShowDialog(this);
            };

            if (InvokeRequired)
                Invoke(invoker);
            else
                invoker();

            return result;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AssignHandles(true);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            AssignHandles(false);
            base.OnHandleDestroyed(e);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            SetBackColor();

            _defaultColor = _parentBackColor;
            SetColors();
        }

        private void SetBackColor()
        {
            Control parent = Parent;

            while (parent != null)
            {
                if (parent is Form)
                {
                    _parentBackColor = parent.BackColor;
                    break;
                }
                parent = parent.Parent;
            }
        }

        private void SetMenu()
        {
            try
            {
                ContextMenuStrip = new ContextMenuStrip();

                ContextMenuStrip.BackColor = BackgroundColor;
                ContextMenuStrip.ForeColor = ForeColor;

                ToolStripItem item = new ToolStripMenuItem("Exportar excel");

                item.Click += (sender, e) => { ExportExcel(); };
                ContextMenuStrip.Items.Add(item);                                
            }
            catch
            {

            }
        }
        
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int currentMouseOverRow = this.HitTest(e.X, e.Y).RowIndex;

                if (currentMouseOverRow >= 0)
                {
                    ContextMenuStrip.Show(this, new Point(e.X, e.Y));
                }
            }
            else
            {
                base.OnMouseClick(e);
            }
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            SetBackColor();
            SetColors();
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            base.OnSelectionChanged(e);

            //if (SelectedRows.Count > 0)
            //{
            //    if (_selectedRowIndex != SelectedRows[0].Index)
            //    {
            //        _selectedRowIndex = SelectedRows[0].Index;
            //        OnCellClick(new DataGridViewCellEventArgs(-1, _selectedRowIndex));
            //    }
            //}
        }

        public void SelectRow(int index)
        {
            if (Rows.Count < (index + 1))
                return;
            try
            {
                if (index < 0)
                    return;

                ClearSelection();
                Rows[index].Selected = true;
            }
            catch
            {

            }
        }

        public new void ClearSelection()
        {
            base.ClearSelection();

            if (Rows.Count > 0)
            {
                Rows[0].Selected = false;

                foreach (DataGridViewCell cel in Rows[0].Cells)
                {
                    cel.Style.BackColor = _defaultColor;
                }
            }
        }

        protected override void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs e)
        {
            base.OnDataBindingComplete(e);

            ClearSelection();
        }

        protected override void OnDataSourceChanged(EventArgs e)
        {
            Action set = () =>
            {
                base.OnDataSourceChanged(e);
                SetColors();
                ClearSelection();
            };

            if (InvokeRequired)
                Invoke(set);
            else
                set();
        }

        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            e.Column.SortMode = DataGridViewColumnSortMode.NotSortable;
            base.OnColumnAdded(e);
        }
        
        private void Initialize()
        {
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            _cellStyle1 = new DataGridViewCellStyle();
            _cellStyle2 = new DataGridViewCellStyle();
            _cellStyle3 = new DataGridViewCellStyle();
            _cellStyle4 = new DataGridViewCellStyle();
            SetColors();
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            AllowUserToAddRows = false;
            BorderStyle = BorderStyle.None;
            CellBorderStyle = DataGridViewCellBorderStyle.None;
            ColumnHeadersDefaultCellStyle = _cellStyle1;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DefaultCellStyle = _cellStyle2;
            Size = new Size(894, 498);
            Dock = DockStyle.Fill;
            EditMode = DataGridViewEditMode.EditProgrammatically;
            EnableHeadersVisualStyles = false;
            Location = new Point(3, 16);
            Name = "dataGridView";
            RowHeadersDefaultCellStyle = _cellStyle3;
            RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            base.RowHeadersVisible = true;
            RowsDefaultCellStyle = _cellStyle4;
            base.RowHeadersWidth = 4;
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ShowCellToolTips = false;
            CellBorderStyle = DataGridViewCellBorderStyle.None;
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
        }

        private void SetColors()
        {
            ColumnHeadersVisible = (Columns.Count > 1);

            if (_initilizedColor || _backColor == Color.Empty)
                return;

            Action<DataGridViewCellStyle, bool> SetColor = (style, bold) =>
            {
                if (style == null)
                    style = new DataGridViewCellStyle();
                style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                style.BackColor = _backColor;
                style.Font = new Font("Microsoft Sans Serif", 8.25F, bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                style.WrapMode = DataGridViewTriState.False;
            };

            BackgroundColor = _backColor;
            GridColor = _backColor;
            SetColor(_cellStyle1, true);
            SetColor(_cellStyle2, false);
            RowHeadersDefaultCellStyle.SelectionBackColor = _backColor;
            SetColor(_cellStyle3, false);
            SetColor(_cellStyle4, false);

            _initilizedColor = (_backColor != _defaultColor);

            SetMenu();
        }

        private void SetForeColor(DataGridViewCellStyle style)
        {
            style.ForeColor = _foreColor;
        }

        private void AssignHandles(bool attach)
        {
            if (attach)
            {
                AssignHandles(false);
                KeyUp += OnKeyUp;
            }
            else
            {
                KeyUp -= OnKeyUp;
            }
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == -1)
            {
                var brush = new SolidBrush(ColumnHeadersDefaultCellStyle.BackColor);
                
                e.Graphics.FillRectangle(brush, e.CellBounds);

                brush.Dispose();

                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentBackground & ~DataGridViewPaintParts.Border);
                ControlPaint.DrawBorder(e.Graphics, e.CellBounds, _backColor, 1, ButtonBorderStyle.Solid, _backColor, 1, ButtonBorderStyle.Solid, GridColor, 1, ButtonBorderStyle.Solid, _backColor, 1, ButtonBorderStyle.Solid);
                
                e.CellStyle.SelectionBackColor = _backColor;
                e.Handled = true;
            }

            e.CellStyle.SelectionBackColor = _selectionBackColor;
            e.CellStyle.SelectionForeColor = _backColor;

            e.CellStyle.ForeColor = _foreColor;
            e.CellStyle.BackColor = _backColor;
        }

        private void OnCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            int columnIndex = 0;

            if (e.RowIndex >= 0 && e.ColumnIndex >= columnIndex)
            {
                if (e.ColumnIndex % 2 == 0)
                {
                    var brush = new SolidBrush(ColumnHeadersDefaultCellStyle.BackColor);

                    e.Graphics.FillRectangle(brush, e.CellBounds);

                    brush.Dispose();

                    e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentBackground);

                    ControlPaint.DrawBorder(e.Graphics, e.CellBounds, Color.Transparent, 1, ButtonBorderStyle.Solid, Color.Transparent, 1, ButtonBorderStyle.Solid, Color.CornflowerBlue, 1, ButtonBorderStyle.Solid, Color.Transparent, 1, ButtonBorderStyle.Solid);

                    e.Handled = true;
                }
            }

            if (e.RowIndex == -1 && e.ColumnIndex >= columnIndex)
            {
                if (e.ColumnIndex % 2 == 0)
                {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);

                    ControlPaint.DrawBorder(e.Graphics, e.CellBounds, Color.Transparent, 1, ButtonBorderStyle.Solid, Color.Transparent, 1, ButtonBorderStyle.Solid, Color.CornflowerBlue, 1, ButtonBorderStyle.Solid, Color.Transparent, 1, ButtonBorderStyle.Solid);

                    e.Handled = true;

                    e.Handled = true;
                }
            }
        }

        protected override void OnGridColorChanged(EventArgs e)
        {
            if (GridColor != Color.Gray)
                GridColor = Color.Gray;
            base.OnGridColorChanged(e);
        }

        protected override void OnCellBorderStyleChanged(EventArgs e)
        {
            if (CellBorderStyle != DataGridViewCellBorderStyle.Single)
            {
                CellBorderStyle = DataGridViewCellBorderStyle.Single;
            }

            base.OnCellBorderStyleChanged(e);
        }

        protected override void OnColumnHeadersBorderStyleChanged(EventArgs e)
        {
            if (ColumnHeadersBorderStyle != DataGridViewHeaderBorderStyle.Single)
            {
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            }

            base.OnColumnHeadersBorderStyleChanged(e);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                    if (SelectedRows.Count > 0)
                    {
                        OnCellClick(new DataGridViewCellEventArgs(-1, SelectedRows[0].Index));
                    }
                    break;
            }
        }

        protected override void OnScroll(ScrollEventArgs e)
        {
            SuspendLayout();
            base.OnScroll(e);
            ResumeLayout();
        }
    }
}

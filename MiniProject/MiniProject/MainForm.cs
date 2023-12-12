using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniProject
{
    public partial class MainForm : Form
    {
        //MDI Child 인스턴스
        DashBoard dashBoard = new DashBoard();

        public MainForm()
        {
            InitializeComponent();

            //MDI 활성화
            IsMdiContainer = true;

            //삭제 예정
            Resize += MainForm_Resize;

            subFormShow(Background.Instance);
            subFormShow(dashBoard);
        }

        //subFormShow 메소드
        private void subFormShow(Form form)
        {
            AnchorStyles anchorStyles = (form == Background.Instance) ? AnchorStyles.Top | AnchorStyles.Left : AnchorStyles.Bottom | AnchorStyles.Left;
            form.Anchor = anchorStyles;
            form.MdiParent = this;
            form.Show();
        }

        //삭제 예정
        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                this.FormBorderStyle = FormBorderStyle.Fixed3D;
                this.TopMost = true;
                dashBoard.MouseClick += MainForm_MouseClick;
            }
            else
            {
                this.TopMost = false;
                dashBoard.MouseClick -= MainForm_MouseClick;
            }
        }
        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }

        }
    }
}

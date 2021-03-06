﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RiziFrame.Utility.Uc;
using RiziFrame.Utility.Db;
using zhiyueBook.Model;
using zhiyueBook.DAL;
using RiziFrame.Utility.Common;

namespace zhiyueBook.WinUI
{
    public partial class TjjyForm : Form
    {
        DbProvider dbProvider = new DbProvider();

        string is_rq_start = System.DateTime.Now.Year.ToString() + "-01-01";
        string is_rq_end = DateTime.Now.ToString("yyyy-MM-dd"); 

        // 创建数据访问对象        
        private TjjyService srv = new TjjyService();

        // 保存查询结果，作为缓存，后面修改和删除的时候都要使用。
        private List<Tjjy> queryList = null;
        private BindingCollection<Tjjy> bcQueryList = null;
        private BindingList<Tjjy> bQueryList = null;        


        public TjjyForm()
        {
            InitializeComponent();

            // 初始化
            this.dgvMain.AutoGenerateColumns = false;
            this.dgvMain.AllowUserToAddRows = false; // 不允许用户向dgv添加行
            this.dgvMain.AllowUserToDeleteRows = false; // 不允许用户向dgv删除行
            this.dgvMain.AllowUserToOrderColumns = true; // 不允许用户向dgv添加行.

            // 冻结字段, 和奇偶行样式冲突
            //DataGridViewHelp.SetColumFrozen(this.cRcode);
            //DataGridViewHelp.SetColumFrozen(this.cName);

            DataGridViewHelp.ColumnReadOnly(this.dgvMain);
            DataGridViewHelp.SetStyleGrid(this.dgvMain); 
            //DataGridViewHelp.SetStyleGrid(this.dgvMain, 40 ); 

            //this.btnEdit.Enabled = this.btnDel.Enabled = false; // 禁用修改和删除按钮

            //设置日期
            this.dtpStart.Text = is_rq_start;
            this.dtpEnd.Text = is_rq_end;
            
            this.InitCbb();
            this.ShowList();
            this.dgvMain.DataSource = bQueryList;
           
        }

        private void InitCbb()
        { 
            ControlHelper.CbbSetDateArea(cbbFindRQ, this.dtpStart, this.dtpEnd);
        }

        // 根据“关键字”查询
        private void QueryByKeyWord()
        {
            // 起止日期
            is_rq_start = this.dtpStart.Text;
            is_rq_end = this.dtpEnd.Text;

            this.ShowList();
            this.dgvMain.DataSource = bQueryList;
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            this.QueryByKeyWord();
        }

        /// <summary>
        /// 显示所有信息
        /// </summary>
        private void ShowList()
        {

            OleDbHelper.SetConn(dbProvider.ConnectionString);

            if (cboStop.Checked)
            {
                this.queryList = srv.Query(1, is_rq_start, is_rq_end, txtKeyWord.Text.Trim());
                //this.bQueryList = (BindingCollection < Tjjy > )srv.Query(1);
            }
            else
            {
                this.queryList = srv.Query(is_rq_start, is_rq_end, txtKeyWord.Text.Trim());
            }
            this.bQueryList = new BindingCollection<Tjjy>(this.queryList);
            //this.dgvMain.DataSource = bQueryList;

            //this.bQueryList = new BindingList<Tjjy>(this.queryList);

            //MessageBox.Show(this.queryList.Count().ToString());
            //this.dgvMain.DataSource = this.queryList;
            // 绑定 BindingCollection
            //this.dgvMain.DataSource = new BindingCollection<Tjjy>(this.queryList);
            //this.dgvMain.ba


            this.ShowState();

        }

        /// <summary>
        /// 显示状态信息
        /// </summary>
        private void ShowState()
        {
            // 显示借阅信息
            int allCount = 0,
                _sumBrBooks = 0,
                _sumRtBooks = 0,
                _notRtBooks = 0,
                normalRd = 0,  // 正常
                stopRd = 0,    // 停用
                expireRd = 0;  // 即将过期
                

            allCount = this.queryList.Count();
            _sumBrBooks = this.queryList.Sum(it => it.SumBrBooks);
            _sumRtBooks = this.queryList.Sum(it => it.SumRtBooks);
            _notRtBooks = this.queryList.Sum(it => it.NotRtBooks);

            normalRd = this.queryList.Count(it => it.State == "正常");
            stopRd = this.queryList.Count(it => it.State == "停用");
            expireRd = this.queryList.Count(it => it.State == "即将过期");

            this.lblAllCount.Text = string.Format("总记录：{0}", allCount.ToString());

            this.lblSumBrBooks.Text = string.Format("借阅总数：{0} 本", _sumBrBooks.ToString());
            this.lblSumRtBooks.Text = string.Format("还书总数：{0} 本", _sumRtBooks.ToString());
            this.lblNotRtBooks.Text = string.Format("未还数：{0} 本", _notRtBooks.ToString());            

            this.lblState.Text = string.Format("其中：正常：{0}人,  停用：{1}人,  即将到期：{2}人",
                normalRd, stopRd, expireRd);
            
        }

        private void dgvMain_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            //双击行时能得到rowIndex
            int index = e.RowIndex;

            // 当前行获取值
            if (this.dgvMain.CurrentRow == null) return;
            DataGridViewRow dgvr = this.dgvMain.CurrentRow;
            string val = dgvr.Cells["cRcode"].Value.ToString();
            //MessageBox.Show(val);

            //利用构造函数传值到弹出页面，并在构造函数中做赋值操作            
            PostTjjy curobj = new PostTjjy();
            curobj.ArgTjjy = new Tjjy();

            curobj.RqStart = this.dtpStart.Text;
            curobj.RqEnd = this.dtpEnd.Text;

            curobj.ArgTjjy.Rcode = dgvr.Cells["cRcode"].Value.ToString();
            curobj.ArgTjjy.Rname = dgvr.Cells["cName"].Value.ToString();
            curobj.ArgTjjy.SumBrBooks = Convert.ToInt32(dgvr.Cells["cSumBrBooks"].Value);
            curobj.ArgTjjy.SumRtBooks = Convert.ToInt32(dgvr.Cells["cSumRtBooks"].Value);
            curobj.ArgTjjy.NotRtBooks = Convert.ToInt32(dgvr.Cells["cNotRtBooks"].Value);

            curobj.ArgTjjy.SumBookPrice = Convert.ToDouble(dgvr.Cells["cSumBookPrice"].Value);

           
            TjjyxxForm frm = new TjjyxxForm(curobj);
            //JyxxForm frm = new JyxxForm(val);
            frm.ShowDialog();
        }

        private void dgvMain_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            DataGridViewColumnHelper.ColStateSetBgc(this.dgvMain, e);
        }

        private void cboAllRq_CheckedChanged(object sender, EventArgs e)
        {
            //if (this.cboAllRq.Checked)
            //{
            //}
            //else
            //{
            //    is_rq_start = System.DateTime.Now.Year.ToString() + "-01-01";
            //    is_rq_end = DateTime.Now.ToString("yyyy-MM-dd"); 
            //}
        }

        private void txtKeyWord_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                this.QueryByKeyWord();
            }
        }
    }
}

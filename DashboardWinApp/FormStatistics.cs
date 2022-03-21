using System;
using System.Drawing;
using System.Windows.Forms;
using DashboardWinApp.Models;

namespace DashboardWinApp
{
    public partial class FormStatistics : Form
    {
        //Fields variables
        readonly Dashboard oModel;
        private Button currButton;
        public FormStatistics()
        {
            InitializeComponent();
            //Default - Last 7 days
            dtpStartDate.Value = DateTime.Today.AddDays(-7);
            dtpEndDate.Value = DateTime.Now;
            btnLast7Days.Select();
            oModel = new Dashboard();
            this.SetDateMenuButtonsUI(btnLast7Days);
            this.LoadData();
        }

        //Private methods
        private void LoadData()
        {
            var refreshData = oModel.LoadData(dtpStartDate.Value, dtpEndDate.Value);
            if (refreshData == true)
            {
                lblNumOrders.Text = oModel.NumOrders.ToString();
                lblTotalRevenue.Text = "$" + oModel.TotalRevenue.ToString();
                lblTotalProfit.Text = "$" + oModel.TotalProfit.ToString();
                lblNumCustomers.Text = oModel.NumCustomers.ToString();
                lblNumSuppliers.Text = oModel.NumSuppliers.ToString();
                lblNumProducts.Text = oModel.NumProducts.ToString();
                chartGrossRevenue.DataSource = oModel.GrossRevenueList;
                chartGrossRevenue.Series[0].XValueMember = "Date";
                chartGrossRevenue.Series[0].YValueMembers = "TotalAmount";
                chartGrossRevenue.DataBind();
                chartTopProducts.DataSource = oModel.TopProductsList;
                chartTopProducts.Series[0].XValueMember = "Key";
                chartTopProducts.Series[0].YValueMembers = "Value";
                chartTopProducts.DataBind();
                dgvUnderstock.DataSource = oModel.UnderstockList;
                dgvUnderstock.Columns[0].HeaderText = "Item";
                dgvUnderstock.Columns[1].HeaderText = "Units";
                Console.WriteLine("Loaded view :)");
            }
            else Console.WriteLine("View not loaded, same query");
        }
        private void SetDateMenuButtonsUI(object button)
        {
            var btn = (Button)button;
            // Highlight button
            btn.BackColor = btnLast30Days.FlatAppearance.BorderColor;
            btn.ForeColor = Color.White;
            // Unhighlight button
            if(currButton!= null && currButton!=btn)
            {
                currButton.BackColor = this.BackColor;
                currButton.ForeColor = Color.FromArgb(124, 141, 181);
            }
            // Set current button
            currButton = btn;

            // Enable Custom Dates
            if (btn == btnCustomDate)
            {
                dtpEndDate.Enabled = true;
                dtpStartDate.Enabled = true;
                btnOkCustomDate.Visible = true;
                lblEndDate.Cursor = Cursors.Hand;
                lblStartDate.Cursor = Cursors.Hand;
            }
            // Disable Custom Dates
            else
            {
                dtpEndDate.Enabled = false;
                dtpStartDate.Enabled = false;
                btnOkCustomDate.Visible = false;
                lblEndDate.Cursor = Cursors.Default;
                lblStartDate.Cursor = Cursors.Default;
            }
        }

        //Event methods
        private void FormStatistics_Load(object sender, EventArgs e)
        {
            lblEndDate.Text = dtpEndDate.Text;
            lblStartDate.Text = dtpStartDate.Text;
            dgvUnderstock.Columns[1].Width = 60;
            dgvUnderstock.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        private void btnToday_Click(object sender, EventArgs e)
        {
            dtpEndDate.Value = DateTime.Now;
            dtpStartDate.Value = DateTime.Now;
            this.LoadData();
            this.SetDateMenuButtonsUI(sender);
        }
        private void btnLast7Days_Click(object sender, EventArgs e)
        {
            dtpEndDate.Value = DateTime.Now;
            dtpStartDate.Value = DateTime.Now.AddDays(-7);
            this.LoadData();
            this.SetDateMenuButtonsUI(sender);
        }
        private void btnLast30Days_Click(object sender, EventArgs e)
        {
            dtpEndDate.Value = DateTime.Now;
            dtpStartDate.Value = DateTime.Now.AddDays(-30);
            this.LoadData();
            this.SetDateMenuButtonsUI(sender);
        }
        private void btnThisMonth_Click(object sender, EventArgs e)
        {
            dtpEndDate.Value = DateTime.Now;
            dtpStartDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            this.LoadData();
            this.SetDateMenuButtonsUI(sender);
        }
        private void btnCustomDate_Click(object sender, EventArgs e)
        {
            this.SetDateMenuButtonsUI(sender);
        }
        private void btnOkCustomDate_Click(object sender, EventArgs e)
        {
            this.LoadData();
        }
        private void lblStartDate_Click(object sender, EventArgs e)
        {
            if(currButton==btnCustomDate)
            {
                dtpStartDate.Select();
                SendKeys.Send("%{DOWN}");
            }
        }
        private void lblEndDate_Click(object sender, EventArgs e)
        {
            if (currButton == btnCustomDate)
            {
                dtpEndDate.Select();
                SendKeys.Send("%{DOWN}");
            }
        }
        private void dtpEndDate_ValueChanged(object sender, EventArgs e)
        {
            lblEndDate.Text = dtpEndDate.Text;
        }
        private void dtpStartDate_ValueChanged(object sender, EventArgs e)
        {
            lblStartDate.Text = dtpStartDate.Text;
        }        
    }
}

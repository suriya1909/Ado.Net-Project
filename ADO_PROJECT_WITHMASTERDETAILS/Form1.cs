using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADO_PROJECT_WITHMASTERDETAILS
{
    public partial class Form1 : Form
    {
        int inEmpId = 0;
        bool isDefaultImage = true;
        string strConnectionString = @"Data Source=DESKTOP-H5PH5U2;Database=Project_DB;Integrated Security=True", strPreviousImage = "";
        OpenFileDialog ofd = new OpenFileDialog();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PositionComboBoxFile();
            FillEmployeeDataGridView();
            Clear();
        }
        void Clear()
        {
            txtEmpCod.Text = txtEmpName.Text = "";
            cmbPosition.SelectedIndex = CmbGender.SelectedIndex = 0;
            dtpDOB.Value = DateTime.Now;
            rbtRegular.Checked = true;
            if (dgvEmpCompany.DataSource == null)
                dgvEmpCompany.Rows.Clear();
            else dgvEmpCompany.DataSource = (dgvEmpCompany.DataSource as DataTable).Clone();
            inEmpId = 0;
            btnSave.Text = "Save";
            btnDelete.Enabled = false;
            pbxPhoto.Image = Image.FromFile(Application.StartupPath + "\\Images\\DefaultImage.png");
            isDefaultImage = false;
        }

        void PositionComboBoxFile()
        {
            using (SqlConnection sqlcon = new SqlConnection(strConnectionString))
            {
                sqlcon.Open();
                SqlDataAdapter sqlda = new SqlDataAdapter("select * from Positions", sqlcon);
                DataTable dtbl = new DataTable();
                sqlda.Fill(dtbl);
                DataRow topItem = dtbl.NewRow();
                topItem[0] = 0;
                topItem[1] = "--Select--";
                dtbl.Rows.InsertAt(topItem, 0);
                cmbPosition.ValueMember = dgvcmbPosition.ValueMember = "PositionId";
                cmbPosition.DisplayMember = dgvcmbPosition.DisplayMember = "Position";
                cmbPosition.DataSource = dtbl;
                dgvcmbPosition.DataSource = dtbl.Copy();
            }
        }
        private void btnImageBrowser_Click(object sender, EventArgs e)
        {
            ofd.Filter = "Images(.jpg,.png) | *.png; *.jpg";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pbxPhoto.Image = new Bitmap(ofd.FileName);

                isDefaultImage = false;
                strPreviousImage = "";
            }
        }

        private void btnImgClear_Click(object sender, EventArgs e)
        {
            pbxPhoto.Image = new Bitmap(Application.StartupPath + "\\Images\\DefaultImage.png");
            isDefaultImage = true;
            strPreviousImage = "";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateMasterDetaailsFor())
            {
                int _EmpID = 0;
                using (SqlConnection sqlcon = new SqlConnection(strConnectionString))
                {
                    sqlcon.Open();
                    SqlCommand sqlcmd = new SqlCommand("EmployeeAddOrEdit", sqlcon);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.Parameters.AddWithValue("@EmpId", inEmpId);
                    sqlcmd.Parameters.AddWithValue("@EmpCod", txtEmpCod.Text.Trim());
                    sqlcmd.Parameters.AddWithValue("@EmpName", txtEmpName.Text.Trim());
                    sqlcmd.Parameters.AddWithValue("@PositionId", Convert.ToInt32(cmbPosition.SelectedValue));
                    sqlcmd.Parameters.AddWithValue("@DOB", dtpDOB.Value);
                    sqlcmd.Parameters.AddWithValue("@Gender", CmbGender.Text);
                    sqlcmd.Parameters.AddWithValue("@Status", rbtRegular.Checked ? "Regular" : "Contractual");
                    if (isDefaultImage)
                        sqlcmd.Parameters.AddWithValue("@ImagePath", DBNull.Value);
                    else if (inEmpId > 0 && strPreviousImage != "")
                        sqlcmd.Parameters.AddWithValue("@ImagePath", strPreviousImage);
                    else
                        sqlcmd.Parameters.AddWithValue("@ImagePath", SaveImage(ofd.FileName));
                    _EmpID = Convert.ToInt32(sqlcmd.ExecuteScalar());
                }

                //Details
                using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    foreach (DataGridViewRow dgvRow in dgvEmpCompany.Rows)
                    {
                        if (dgvRow.IsNewRow) break;
                        else
                        {
                            SqlCommand sqlCmd = new SqlCommand("CompanyAddOrEdit", sqlCon);
                            sqlCmd.CommandType = CommandType.StoredProcedure;
                            sqlCmd.Parameters.AddWithValue("@EmpCmpId", Convert.ToInt32(dgvRow.Cells["dgvtxtEmpCmpId"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvtxtEmpCmpId"].Value));
                            sqlCmd.Parameters.AddWithValue("@EmpId", _EmpID);
                            sqlCmd.Parameters.AddWithValue("@EmpCmpName", dgvRow.Cells["dgvtxtCompanyName"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvtxtCompanyName"].Value);
                            sqlCmd.Parameters.AddWithValue("@positionId", Convert.ToInt32(dgvRow.Cells["dgvcmbPosition"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvcmbPosition"].Value));
                            sqlCmd.Parameters.AddWithValue("@YearOfExp", Convert.ToInt32(dgvRow.Cells["dgvtxtYear"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvtxtYear"].Value));
                            sqlCmd.ExecuteNonQuery();
                        }
                    }
                }
                FillEmployeeDataGridView();
                Clear();
                MessageBox.Show("Submitted Successfully");
            }
        }
        private bool ValidateMasterDetaailsFor()
        {
            bool _IsValid = true;
            if (txtEmpName.Text.Trim() == "")
            {
                MessageBox.Show("Employee Name is Required");
                _IsValid = false;
            }

            return _IsValid;
        }
        private string SaveImage(string _ImagePath)
        {
            string _FileName = Path.GetFileNameWithoutExtension(_ImagePath);
            string _Extention = Path.GetExtension(_FileName);
            _FileName = _FileName.Length <= 15 ? _FileName : _FileName.Substring(0, 15);
            _FileName = _FileName + DateTime.Now.ToString("yymmssff") + _Extention;
            pbxPhoto.Image.Save(Application.StartupPath + "\\Images\\" + _FileName);
            return _FileName;
        }

        private void dgvEmployee_DoubleClick(object sender, EventArgs e)
        {
            if (dgvEmployee.CurrentRow.Index != -1)
            {
                DataGridViewRow _dgvCurrentRow = dgvEmployee.CurrentRow;
                inEmpId = Convert.ToInt32(_dgvCurrentRow.Cells[0].Value);
                using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("EmployeeViewById", sqlCon);
                    sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sqlDa.SelectCommand.Parameters.AddWithValue("@EmpId", inEmpId);

                    DataSet ds = new DataSet();
                    sqlDa.Fill(ds);

                    //Master - Fill
                    DataRow dr = ds.Tables[0].Rows[0];
                    txtEmpCod.Text = dr["EmpCod"].ToString();
                    txtEmpName.Text = dr["EmpName"].ToString();
                    cmbPosition.SelectedValue = Convert.ToInt32(dr["PositionId"].ToString());
                    dtpDOB.Value = Convert.ToDateTime(dr["DOB"].ToString());
                    CmbGender.Text = dr["Gender"].ToString();
                    if (dr["Status"].ToString() == "Regular")
                        rbtRegular.Checked = true;

                    else rbtContractual.Checked = true;

                    if (dr["ImagePath"] == DBNull.Value)
                    {
                        pbxPhoto.Image = new Bitmap(Application.StartupPath + "\\Images\\DefaultImage.png");
                        isDefaultImage = true;
                    }
                    else
                    {
                        pbxPhoto.Image = new Bitmap(Application.StartupPath + "\\Images\\" + dr["ImagePath"].ToString());
                        strPreviousImage = dr["ImagePath"].ToString();
                        isDefaultImage = true;
                    }
                    dgvEmpCompany.AutoGenerateColumns = false;
                    dgvEmpCompany.DataSource = ds.Tables[1];
                    btnDelete.Enabled = true;
                    btnSave.Text = "Update";
                    tabControl1.SelectedIndex = 0;
                }
            }
        }
        private void dgvEmpCompany_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dgvRow = dgvEmpCompany.CurrentRow;
            if (dgvRow.Cells["dgvtxtEmpCmpId"].Value != DBNull.Value)
            {
                if (MessageBox.Show("Are You Sure To Delete This Record?", "Master Details CRUD", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                    {
                        sqlCon.Open();
                        SqlCommand sqlcmd = new SqlCommand("CompanyDelete", sqlCon);
                        sqlcmd.CommandType = CommandType.StoredProcedure;
                        sqlcmd.Parameters.AddWithValue("@EmpEmpId", Convert.ToInt32(dgvRow.Cells["dgvtxtEmpCmpId"].Value));
                        sqlcmd.ExecuteNonQuery();
                    }
                }
                else
                    e.Cancel = true;
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are You Sure To Delete This Record?", "Master Details CRUD", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    SqlCommand sqlcmd = new SqlCommand("EmployeeDelete", sqlCon);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.Parameters.AddWithValue("@EmpId", inEmpId);
                    sqlcmd.ExecuteNonQuery();
                    Clear();
                    FillEmployeeDataGridView();
                    MessageBox.Show("Deleted Successfully");
                };

            }
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            frmreport frmreport = new frmreport();
            frmreport.ShowDialog();
        }

        private void FillEmployeeDataGridView()
        {
            using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
            {

                sqlCon.Open();

                SqlDataAdapter sqlDa = new SqlDataAdapter("EmployeeViewAll", sqlCon);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dtbl = new DataTable();
                sqlDa.Fill(dtbl);
                dgvEmployee.DataSource = dtbl;
                dgvEmployee.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvEmployee.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvEmployee.Columns[0].Visible = false;
            }
        }
    }
}

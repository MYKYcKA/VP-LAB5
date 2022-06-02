﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Lab3
{
    public partial class Form1 : Form
    {
        List<EmployeeDTO> employeeList = new List<EmployeeDTO>();
        bool IsValid = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
        private void LoadFromFile(string path)
        {
            employeeList = new List<EmployeeDTO>();
            IsValid = true;
            var readerSettings = new XmlReaderSettings();
            readerSettings.Schemas.Add("My-namespace", "validation.xsd");
            readerSettings.ValidationType = ValidationType.Schema;
            readerSettings.ValidationEventHandler += new ValidationEventHandler((sender,e)=> EmployeeSettingsValidationEventHandler(sender,e,this));

            XmlReader reader = XmlReader.Create(path, readerSettings);

            reader.ReadToFollowing("EmployeeDTO");
            do
            {
                var employee = new EmployeeDTO();
                foreach (var propertie in employee.GetType().GetProperties())
                {
                    reader.ReadToFollowing(propertie.Name);
                    propertie.SetValue(employee, reader.ReadElementContentAsString());
                    if (propertie.Name.Equals("Appartment"))
                        break;
                }
                if (!IsValid)
                {
                    employeeList = null;
                    break;
                }
                employeeList.Add(employee);
            } while (reader.ReadToFollowing("EmployeeDTO"));
            reader.Close();
        }


        private void SaveToFile(List<EmployeeDTO> listofa, string path)
        {
            var outFile = File.Create(path);
            var formatter = new XmlSerializer(typeof(List<EmployeeDTO>));
            formatter.Serialize(outFile, listofa);
            outFile.Close();
        }

        private void AddClick(object sender, EventArgs e)
        {
            var employee = new EmployeeDTO
            {
                Name = NameTextBox.Text,
                Surname = SurnameTextBox.Text,
                Address = AddressTextBox.Text,
                Appartment = AppartmenttextBox.Text,
                BirthDate = BirthdateTextBox.Text,
                Building = BuildingtextBox1.Text,
                City = CitytextBox.Text,
                PhoneNumber = PhoneNumberTextBox.Text,
                Street = StreettextBox.Text,
            };
            employeeList.Add(employee);
            dataGridView1.DataSource = typeof(List<EmployeeDTO>);
            dataGridView1.DataSource = employeeList;
            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                    if (control is TextBox)
                        (control as TextBox).Clear();
                    else
                        func(control.Controls);
            };

            func(Controls);
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                employeeList.Remove(row.DataBoundItem as EmployeeDTO);
            }
            dataGridView1.DataSource = typeof(List<EmployeeDTO>);
            dataGridView1.DataSource = employeeList;
        }

        private void SearchClick(object sender, EventArgs e)
        {
            var form2 = new Form2();
            var selected = employeeList.Where(x => x.GetType().GetProperty(comboBox1.SelectedItem as string).GetValue(x, null).Equals(SearchtextBox1.Text)).ToList();
            form2.employeeList = selected;
            form2.dataGridView1.DataSource = selected;
            form2.dataGridView1.Columns[0].Visible = NamecheckBox1.Checked;
            form2.dataGridView1.Columns[1].Visible = SurnamecheckBox2.Checked;
            form2.dataGridView1.Columns[2].Visible = BirthDatecheckBox1.Checked;
            form2.dataGridView1.Columns[3].Visible = PhoneNumbercheckBox2.Checked;
            form2.dataGridView1.Columns[4].Visible = AddresscheckBox4.Checked;
            form2.dataGridView1.Columns[5].Visible = CitycheckBox3.Checked;
            form2.dataGridView1.Columns[6].Visible = StreetcheckBox2.Checked;
            form2.dataGridView1.Columns[7].Visible = BuildingcheckBox1.Checked;
            form2.dataGridView1.Columns[8].Visible = AppartmentcheckBox5.Checked;
            var averageAge = 0;
            foreach (var employee in selected)
            {
                DateTime zeroTime = new DateTime(1, 1, 1);
                var birtdate = DateTime.Parse(employee.BirthDate);
                var age = DateTime.Now - birtdate;
                var years = (zeroTime + age).Year - 1;
                averageAge += years;
            }
            if (selected.Count != 0)
                averageAge /= selected.Count;
            form2.label1.Text += $" {averageAge}";
            form2.ShowDialog();
        }

        private void LoadFileClick(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            LoadFromFile(openFileDialog1.FileName);
            dataGridView1.DataSource = typeof(List<EmployeeDTO>);
            dataGridView1.DataSource = employeeList;
        }

        private void SaveFileClick(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            SaveToFile(employeeList, saveFileDialog1.FileName);
        }

        private void NOPTextBox_Validating(object sender, CancelEventArgs e)
        {
            var textBox = sender as TextBox;
            var date = textBox.Text;
            DateTime dDate;
            if (!DateTime.TryParse(date, out dDate))
            {
                e.Cancel = true;
                textBox.Focus();
                errorProvider1.SetError(textBox, "Invalid date!");
                AddButton.Enabled = false;
            }
            else
            {
                AddButton.Enabled = true;
                e.Cancel = false;
                errorProvider1.SetError(textBox, "");
            }
        }

        static void EmployeeSettingsValidationEventHandler(object sender, ValidationEventArgs e, object form)
        {
            var form1 = form as Form1;
            if (e.Severity == XmlSeverityType.Error)
            {
                form1.IsValid = false;
                var errorForm = new XmlErrorForm();
                errorForm.Show();
            }
        }
    }
}

namespace jpeg
{
	partial class Window
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnSelectFile = new System.Windows.Forms.Button();
			this.btnCompress = new System.Windows.Forms.Button();
			this.txtFilePath = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txtSaveLocation = new System.Windows.Forms.TextBox();
			this.btnChooseFolder = new System.Windows.Forms.Button();
			this.lblWorking = new System.Windows.Forms.Label();
			this.lsData = new System.Windows.Forms.ListBox();
			this.lblresults = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnSelectFile
			// 
			this.btnSelectFile.Location = new System.Drawing.Point(12, 42);
			this.btnSelectFile.Name = "btnSelectFile";
			this.btnSelectFile.Size = new System.Drawing.Size(92, 23);
			this.btnSelectFile.TabIndex = 0;
			this.btnSelectFile.Text = "Select File";
			this.btnSelectFile.UseVisualStyleBackColor = true;
			this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
			// 
			// btnCompress
			// 
			this.btnCompress.Location = new System.Drawing.Point(110, 42);
			this.btnCompress.Name = "btnCompress";
			this.btnCompress.Size = new System.Drawing.Size(75, 23);
			this.btnCompress.TabIndex = 2;
			this.btnCompress.Text = "Compress";
			this.btnCompress.UseVisualStyleBackColor = true;
			this.btnCompress.Click += new System.EventHandler(this.btnCompress_Click);
			// 
			// txtFilePath
			// 
			this.txtFilePath.Location = new System.Drawing.Point(120, 12);
			this.txtFilePath.Name = "txtFilePath";
			this.txtFilePath.Size = new System.Drawing.Size(326, 20);
			this.txtFilePath.TabIndex = 1;
			this.txtFilePath.Text = "D:\\code\\java\\JPEG\\images\\Original\\BlackSquareSmall.jpg";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(101, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Image To Compress";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(21, 105);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Save location";
			// 
			// txtSaveLocation
			// 
			this.txtSaveLocation.Location = new System.Drawing.Point(120, 102);
			this.txtSaveLocation.Name = "txtSaveLocation";
			this.txtSaveLocation.Size = new System.Drawing.Size(326, 20);
			this.txtSaveLocation.TabIndex = 4;
			this.txtSaveLocation.Text = "D:\\code\\java\\JPEG\\images\\Compressed";
			// 
			// btnChooseFolder
			// 
			this.btnChooseFolder.Location = new System.Drawing.Point(12, 132);
			this.btnChooseFolder.Name = "btnChooseFolder";
			this.btnChooseFolder.Size = new System.Drawing.Size(92, 23);
			this.btnChooseFolder.TabIndex = 6;
			this.btnChooseFolder.Text = "Choose Folder";
			this.btnChooseFolder.UseVisualStyleBackColor = true;
			this.btnChooseFolder.Click += new System.EventHandler(this.btnChooseFolder_Click);
			// 
			// lblWorking
			// 
			this.lblWorking.AutoSize = true;
			this.lblWorking.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblWorking.Location = new System.Drawing.Point(12, 182);
			this.lblWorking.Name = "lblWorking";
			this.lblWorking.Size = new System.Drawing.Size(0, 20);
			this.lblWorking.TabIndex = 7;
			// 
			// lsData
			// 
			this.lsData.FormattingEnabled = true;
			this.lsData.Location = new System.Drawing.Point(12, 224);
			this.lsData.Name = "lsData";
			this.lsData.Size = new System.Drawing.Size(434, 199);
			this.lsData.TabIndex = 8;
			// 
			// lblresults
			// 
			this.lblresults.AutoSize = true;
			this.lblresults.Location = new System.Drawing.Point(13, 208);
			this.lblresults.Name = "lblresults";
			this.lblresults.Size = new System.Drawing.Size(42, 13);
			this.lblresults.TabIndex = 9;
			this.lblresults.Text = "Results";
			// 
			// Window
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(458, 450);
			this.Controls.Add(this.lblresults);
			this.Controls.Add(this.lsData);
			this.Controls.Add(this.lblWorking);
			this.Controls.Add(this.btnChooseFolder);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtSaveLocation);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnCompress);
			this.Controls.Add(this.txtFilePath);
			this.Controls.Add(this.btnSelectFile);
			this.Name = "Window";
			this.Text = "JPEG Compression";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnSelectFile;
		private System.Windows.Forms.Button btnCompress;
		private System.Windows.Forms.TextBox txtFilePath;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtSaveLocation;
		private System.Windows.Forms.Button btnChooseFolder;
		private System.Windows.Forms.Label lblWorking;
		private System.Windows.Forms.ListBox lsData;
		private System.Windows.Forms.Label lblresults;
	}
}


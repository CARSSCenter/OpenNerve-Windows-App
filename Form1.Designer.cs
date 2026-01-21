using ScottPlot;
using System.Diagnostics;

namespace controller
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            bDisconnect = new Button();
            txtPA = new TextBox();
            labelOpcode = new System.Windows.Forms.Label();
            bSendPA = new Button();
            label1 = new System.Windows.Forms.Label();
            bSendPW = new Button();
            txtPW = new TextBox();
            bSendPF = new Button();
            txtPF = new TextBox();
            label2 = new System.Windows.Forms.Label();
            bStimOff = new Button();
            bStimOn = new Button();
            bSendTN = new Button();
            txtTN = new TextBox();
            label3 = new System.Windows.Forms.Label();
            bSendPR = new Button();
            txtPR = new TextBox();
            label4 = new System.Windows.Forms.Label();
            bGet = new Button();
            labelRX = new System.Windows.Forms.Label();
            labelTX = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            bSendTF = new Button();
            txtTF = new TextBox();
            label7 = new System.Windows.Forms.Label();
            bGetTF = new Button();
            bGetTN = new Button();
            bGetPR = new Button();
            bGetPF = new Button();
            bGetPW = new Button();
            bGetPA = new Button();
            bGetC1 = new Button();
            bSendC1 = new Button();
            label8 = new System.Windows.Forms.Label();
            txtC1 = new TextBox();
            bGetC2 = new Button();
            bSendC2 = new Button();
            label9 = new System.Windows.Forms.Label();
            txtC2 = new TextBox();
            formsPlot1 = new ScottPlot.WinForms.FormsPlot();
            rbEMG2 = new RadioButton();
            rbEMG1 = new RadioButton();
            txtFname = new TextBox();
            bSave = new Button();
            bView = new Button();
            rbEMG3 = new RadioButton();
            rbEMG4 = new RadioButton();
            labelV = new System.Windows.Forms.Label();
            groupSignalMode = new GroupBox();
            getImp = new Button();
            labelZ = new System.Windows.Forms.Label();
            groupSineWave = new GroupBox();
            bSetMaxBlock = new Button();
            txtMaxVNB = new TextBox();
            label17 = new System.Windows.Forms.Label();
            bBlockMax = new Button();
            bGetV2 = new Button();
            bSendV2 = new Button();
            label10 = new System.Windows.Forms.Label();
            txtV2 = new TextBox();
            bGetV1 = new Button();
            bSendV1 = new Button();
            label11 = new System.Windows.Forms.Label();
            txtV1 = new TextBox();
            bGetVoff = new Button();
            bGetVon = new Button();
            bGetVfreq = new Button();
            bGetVamp = new Button();
            bSendVoff = new Button();
            txtVoff = new TextBox();
            label12 = new System.Windows.Forms.Label();
            bSendVon = new Button();
            txtVon = new TextBox();
            label13 = new System.Windows.Forms.Label();
            bSendVfreq = new Button();
            txtVfreq = new TextBox();
            label14 = new System.Windows.Forms.Label();
            bSendVamp = new Button();
            label15 = new System.Windows.Forms.Label();
            txtVAmp = new TextBox();
            bStimMax = new Button();
            bZmin = new Button();
            ckVNB = new CheckBox();
            groupSignalMode.SuspendLayout();
            groupSineWave.SuspendLayout();
            SuspendLayout();
            // 
            // bDisconnect
            // 
            bDisconnect.Enabled = false;
            bDisconnect.Font = new Font("Times New Roman", 14F);
            bDisconnect.Location = new Point(18, 5);
            bDisconnect.Margin = new Padding(4, 5, 4, 5);
            bDisconnect.Name = "bDisconnect";
            bDisconnect.Size = new Size(175, 42);
            bDisconnect.TabIndex = 16;
            bDisconnect.Text = "Quit";
            bDisconnect.TextAlign = ContentAlignment.BottomCenter;
            bDisconnect.UseVisualStyleBackColor = true;
            bDisconnect.Click += bDisconnect_Click;
            // 
            // txtPA
            // 
            txtPA.Font = new Font("Times New Roman", 12F);
            txtPA.Location = new Point(18, 446);
            txtPA.Margin = new Padding(4, 5, 4, 5);
            txtPA.Name = "txtPA";
            txtPA.Size = new Size(72, 30);
            txtPA.TabIndex = 17;
            txtPA.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelOpcode
            // 
            labelOpcode.Font = new Font("Times New Roman", 14F);
            labelOpcode.Location = new Point(-1, 412);
            labelOpcode.Margin = new Padding(4, 0, 4, 0);
            labelOpcode.MaximumSize = new Size(200, 33);
            labelOpcode.MinimumSize = new Size(240, 33);
            labelOpcode.Name = "labelOpcode";
            labelOpcode.Size = new Size(240, 33);
            labelOpcode.TabIndex = 18;
            labelOpcode.Text = "Amp, 0-0.2-5";
            labelOpcode.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // bSendPA
            // 
            bSendPA.Enabled = false;
            bSendPA.Font = new Font("Times New Roman", 14F);
            bSendPA.Location = new Point(98, 443);
            bSendPA.Margin = new Padding(4, 5, 4, 5);
            bSendPA.Name = "bSendPA";
            bSendPA.Size = new Size(71, 40);
            bSendPA.TabIndex = 19;
            bSendPA.Text = "set";
            bSendPA.UseVisualStyleBackColor = true;
            bSendPA.Click += bSendPA_Click;
            // 
            // label1
            // 
            label1.Font = new Font("Times New Roman", 14F);
            label1.Location = new Point(-1, 483);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.MaximumSize = new Size(200, 33);
            label1.MinimumSize = new Size(240, 33);
            label1.Name = "label1";
            label1.Size = new Size(240, 33);
            label1.TabIndex = 20;
            label1.Text = "Width,100-1000";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // bSendPW
            // 
            bSendPW.Enabled = false;
            bSendPW.Font = new Font("Times New Roman", 14F);
            bSendPW.Location = new Point(98, 518);
            bSendPW.Margin = new Padding(4, 5, 4, 5);
            bSendPW.Name = "bSendPW";
            bSendPW.Size = new Size(71, 40);
            bSendPW.TabIndex = 22;
            bSendPW.Text = "set";
            bSendPW.UseVisualStyleBackColor = true;
            bSendPW.Click += bSendPW_Click;
            // 
            // txtPW
            // 
            txtPW.Font = new Font("Times New Roman", 12F);
            txtPW.Location = new Point(18, 521);
            txtPW.Margin = new Padding(4, 5, 4, 5);
            txtPW.Name = "txtPW";
            txtPW.Size = new Size(72, 30);
            txtPW.TabIndex = 21;
            txtPW.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // bSendPF
            // 
            bSendPF.Enabled = false;
            bSendPF.Font = new Font("Times New Roman", 14F);
            bSendPF.Location = new Point(98, 592);
            bSendPF.Margin = new Padding(4, 5, 4, 5);
            bSendPF.Name = "bSendPF";
            bSendPF.Size = new Size(71, 40);
            bSendPF.TabIndex = 25;
            bSendPF.Text = "set";
            bSendPF.UseVisualStyleBackColor = true;
            bSendPF.Click += bSendPF_Click;
            // 
            // txtPF
            // 
            txtPF.Font = new Font("Times New Roman", 12F);
            txtPF.Location = new Point(18, 596);
            txtPF.Margin = new Padding(4, 5, 4, 5);
            txtPF.Name = "txtPF";
            txtPF.Size = new Size(72, 30);
            txtPF.TabIndex = 24;
            txtPF.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            label2.Font = new Font("Times New Roman", 14F);
            label2.Location = new Point(-1, 558);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.MaximumSize = new Size(200, 33);
            label2.MinimumSize = new Size(240, 33);
            label2.Name = "label2";
            label2.Size = new Size(240, 33);
            label2.TabIndex = 23;
            label2.Text = "Freq, 1-1200";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // bStimOff
            // 
            bStimOff.Enabled = false;
            bStimOff.Font = new Font("Times New Roman", 14F);
            bStimOff.Location = new Point(263, 870);
            bStimOff.Margin = new Padding(4, 5, 4, 5);
            bStimOff.Name = "bStimOff";
            bStimOff.Size = new Size(168, 47);
            bStimOff.TabIndex = 27;
            bStimOff.Text = "Stim Off";
            bStimOff.UseVisualStyleBackColor = true;
            bStimOff.Click += bStimOff_Click;
            // 
            // bStimOn
            // 
            bStimOn.Enabled = false;
            bStimOn.Font = new Font("Times New Roman", 14F);
            bStimOn.Location = new Point(91, 870);
            bStimOn.Margin = new Padding(4, 5, 4, 5);
            bStimOn.Name = "bStimOn";
            bStimOn.Size = new Size(160, 47);
            bStimOn.TabIndex = 26;
            bStimOn.Text = "Stim On";
            bStimOn.UseVisualStyleBackColor = true;
            bStimOn.Click += bStimOn_Click;
            // 
            // bSendTN
            // 
            bSendTN.Enabled = false;
            bSendTN.Font = new Font("Times New Roman", 14F);
            bSendTN.Location = new Point(98, 744);
            bSendTN.Margin = new Padding(4, 5, 4, 5);
            bSendTN.Name = "bSendTN";
            bSendTN.Size = new Size(71, 40);
            bSendTN.TabIndex = 34;
            bSendTN.Text = "set";
            bSendTN.UseVisualStyleBackColor = true;
            bSendTN.Click += bSendTN_Click;
            // 
            // txtTN
            // 
            txtTN.Font = new Font("Times New Roman", 12F);
            txtTN.Location = new Point(18, 747);
            txtTN.Margin = new Padding(4, 5, 4, 5);
            txtTN.Name = "txtTN";
            txtTN.Size = new Size(72, 30);
            txtTN.TabIndex = 33;
            txtTN.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            label3.Font = new Font("Times New Roman", 14F);
            label3.Location = new Point(-1, 709);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.MaximumSize = new Size(200, 33);
            label3.MinimumSize = new Size(240, 33);
            label3.Name = "label3";
            label3.Size = new Size(240, 33);
            label3.TabIndex = 32;
            label3.Text = "TrainOn, 10-300";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // bSendPR
            // 
            bSendPR.Enabled = false;
            bSendPR.Font = new Font("Times New Roman", 14F);
            bSendPR.Location = new Point(98, 667);
            bSendPR.Margin = new Padding(4, 5, 4, 5);
            bSendPR.Name = "bSendPR";
            bSendPR.Size = new Size(71, 40);
            bSendPR.TabIndex = 31;
            bSendPR.Text = "set";
            bSendPR.UseVisualStyleBackColor = true;
            bSendPR.Click += bSendPR_Click;
            // 
            // txtPR
            // 
            txtPR.Font = new Font("Times New Roman", 12F);
            txtPR.Location = new Point(18, 670);
            txtPR.Margin = new Padding(4, 5, 4, 5);
            txtPR.Name = "txtPR";
            txtPR.Size = new Size(72, 30);
            txtPR.TabIndex = 30;
            txtPR.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label4
            // 
            label4.Font = new Font("Times New Roman", 14F);
            label4.Location = new Point(-1, 632);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.MaximumSize = new Size(200, 33);
            label4.MinimumSize = new Size(240, 33);
            label4.Name = "label4";
            label4.Size = new Size(240, 33);
            label4.TabIndex = 29;
            label4.Text = "Ramp, 1-10";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // bGet
            // 
            bGet.Enabled = false;
            bGet.Font = new Font("Times New Roman", 14F);
            bGet.Location = new Point(15, 224);
            bGet.Margin = new Padding(4, 5, 4, 5);
            bGet.Name = "bGet";
            bGet.Size = new Size(489, 40);
            bGet.TabIndex = 35;
            bGet.Text = "Get Params";
            bGet.TextAlign = ContentAlignment.BottomCenter;
            bGet.UseVisualStyleBackColor = true;
            bGet.Click += bGet_Click;
            // 
            // labelRX
            // 
            labelRX.Font = new Font("Times New Roman", 12F);
            labelRX.Location = new Point(60, 959);
            labelRX.Margin = new Padding(4, 0, 4, 0);
            labelRX.MinimumSize = new Size(140, 34);
            labelRX.Name = "labelRX";
            labelRX.Size = new Size(190, 34);
            labelRX.TabIndex = 36;
            labelRX.Text = "-";
            labelRX.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelTX
            // 
            labelTX.Font = new Font("Times New Roman", 12F);
            labelTX.Location = new Point(60, 922);
            labelTX.Margin = new Padding(4, 0, 4, 0);
            labelTX.MinimumSize = new Size(140, 34);
            labelTX.Name = "labelTX";
            labelTX.Size = new Size(190, 34);
            labelTX.TabIndex = 37;
            labelTX.Text = "-";
            labelTX.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            label5.Font = new Font("Times New Roman", 12F);
            label5.Location = new Point(6, 921);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.MinimumSize = new Size(40, 34);
            label5.Name = "label5";
            label5.Size = new Size(53, 34);
            label5.TabIndex = 39;
            label5.Text = "TX";
            label5.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            label6.Font = new Font("Times New Roman", 12F);
            label6.Location = new Point(6, 958);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.MinimumSize = new Size(40, 34);
            label6.Name = "label6";
            label6.Size = new Size(53, 34);
            label6.TabIndex = 38;
            label6.Text = "RX";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // bSendTF
            // 
            bSendTF.Enabled = false;
            bSendTF.Font = new Font("Times New Roman", 14F);
            bSendTF.Location = new Point(98, 821);
            bSendTF.Margin = new Padding(4, 5, 4, 5);
            bSendTF.Name = "bSendTF";
            bSendTF.Size = new Size(71, 40);
            bSendTF.TabIndex = 42;
            bSendTF.Text = "set";
            bSendTF.UseVisualStyleBackColor = true;
            bSendTF.Click += bSendTF_Click;
            // 
            // txtTF
            // 
            txtTF.Font = new Font("Times New Roman", 12F);
            txtTF.Location = new Point(18, 825);
            txtTF.Margin = new Padding(4, 5, 4, 5);
            txtTF.Name = "txtTF";
            txtTF.Size = new Size(72, 30);
            txtTF.TabIndex = 41;
            txtTF.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label7
            // 
            label7.Font = new Font("Times New Roman", 14F);
            label7.Location = new Point(-1, 785);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.MaximumSize = new Size(200, 33);
            label7.MinimumSize = new Size(240, 33);
            label7.Name = "label7";
            label7.Size = new Size(240, 33);
            label7.TabIndex = 40;
            label7.Text = "TrainOff, 0-300";
            label7.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // bGetTF
            // 
            bGetTF.Enabled = false;
            bGetTF.Font = new Font("Times New Roman", 14F);
            bGetTF.Location = new Point(174, 821);
            bGetTF.Margin = new Padding(4, 5, 4, 5);
            bGetTF.Name = "bGetTF";
            bGetTF.Size = new Size(64, 40);
            bGetTF.TabIndex = 48;
            bGetTF.Text = "get";
            bGetTF.UseVisualStyleBackColor = true;
            bGetTF.Click += bGetTF_Click;
            // 
            // bGetTN
            // 
            bGetTN.Enabled = false;
            bGetTN.Font = new Font("Times New Roman", 14F);
            bGetTN.Location = new Point(174, 744);
            bGetTN.Margin = new Padding(4, 5, 4, 5);
            bGetTN.Name = "bGetTN";
            bGetTN.Size = new Size(64, 40);
            bGetTN.TabIndex = 47;
            bGetTN.Text = "get";
            bGetTN.UseVisualStyleBackColor = true;
            bGetTN.Click += bGetTN_Click;
            // 
            // bGetPR
            // 
            bGetPR.Enabled = false;
            bGetPR.Font = new Font("Times New Roman", 14F);
            bGetPR.Location = new Point(174, 667);
            bGetPR.Margin = new Padding(4, 5, 4, 5);
            bGetPR.Name = "bGetPR";
            bGetPR.Size = new Size(64, 40);
            bGetPR.TabIndex = 46;
            bGetPR.Text = "get";
            bGetPR.UseVisualStyleBackColor = true;
            bGetPR.Click += bGetPR_Click;
            // 
            // bGetPF
            // 
            bGetPF.Enabled = false;
            bGetPF.Font = new Font("Times New Roman", 14F);
            bGetPF.Location = new Point(174, 592);
            bGetPF.Margin = new Padding(4, 5, 4, 5);
            bGetPF.Name = "bGetPF";
            bGetPF.Size = new Size(64, 40);
            bGetPF.TabIndex = 45;
            bGetPF.Text = "get";
            bGetPF.UseVisualStyleBackColor = true;
            bGetPF.Click += bGetPF_Click;
            // 
            // bGetPW
            // 
            bGetPW.Enabled = false;
            bGetPW.Font = new Font("Times New Roman", 14F);
            bGetPW.Location = new Point(174, 518);
            bGetPW.Margin = new Padding(4, 5, 4, 5);
            bGetPW.Name = "bGetPW";
            bGetPW.Size = new Size(64, 40);
            bGetPW.TabIndex = 44;
            bGetPW.Text = "get";
            bGetPW.UseVisualStyleBackColor = true;
            bGetPW.Click += bGetPW_Click;
            // 
            // bGetPA
            // 
            bGetPA.Enabled = false;
            bGetPA.Font = new Font("Times New Roman", 14F);
            bGetPA.Location = new Point(174, 443);
            bGetPA.Margin = new Padding(4, 5, 4, 5);
            bGetPA.Name = "bGetPA";
            bGetPA.Size = new Size(64, 40);
            bGetPA.TabIndex = 43;
            bGetPA.Text = "get";
            bGetPA.UseVisualStyleBackColor = true;
            bGetPA.Click += bGetPA_Click;
            // 
            // bGetC1
            // 
            bGetC1.Enabled = false;
            bGetC1.Font = new Font("Times New Roman", 14F);
            bGetC1.Location = new Point(174, 300);
            bGetC1.Margin = new Padding(4, 5, 4, 5);
            bGetC1.Name = "bGetC1";
            bGetC1.Size = new Size(64, 40);
            bGetC1.TabIndex = 52;
            bGetC1.Text = "get";
            bGetC1.UseVisualStyleBackColor = true;
            bGetC1.Click += bGetC1_Click;
            // 
            // bSendC1
            // 
            bSendC1.Enabled = false;
            bSendC1.Font = new Font("Times New Roman", 14F);
            bSendC1.Location = new Point(98, 300);
            bSendC1.Margin = new Padding(4, 5, 4, 5);
            bSendC1.Name = "bSendC1";
            bSendC1.Size = new Size(71, 40);
            bSendC1.TabIndex = 51;
            bSendC1.Text = "set";
            bSendC1.UseVisualStyleBackColor = true;
            bSendC1.Click += bSendC1_Click;
            // 
            // label8
            // 
            label8.Font = new Font("Times New Roman", 14F);
            label8.Location = new Point(-1, 269);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.MaximumSize = new Size(200, 33);
            label8.MinimumSize = new Size(240, 33);
            label8.Name = "label8";
            label8.Size = new Size(240, 33);
            label8.TabIndex = 50;
            label8.Text = "Cathode, 1-7";
            label8.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtC1
            // 
            txtC1.Font = new Font("Times New Roman", 12F);
            txtC1.Location = new Point(18, 303);
            txtC1.Margin = new Padding(4, 5, 4, 5);
            txtC1.Name = "txtC1";
            txtC1.Size = new Size(72, 30);
            txtC1.TabIndex = 49;
            txtC1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // bGetC2
            // 
            bGetC2.Enabled = false;
            bGetC2.Font = new Font("Times New Roman", 14F);
            bGetC2.Location = new Point(174, 371);
            bGetC2.Margin = new Padding(4, 5, 4, 5);
            bGetC2.Name = "bGetC2";
            bGetC2.Size = new Size(64, 40);
            bGetC2.TabIndex = 56;
            bGetC2.Text = "get";
            bGetC2.UseVisualStyleBackColor = true;
            bGetC2.Click += bGetC2_Click;
            // 
            // bSendC2
            // 
            bSendC2.Enabled = false;
            bSendC2.Font = new Font("Times New Roman", 14F);
            bSendC2.Location = new Point(98, 371);
            bSendC2.Margin = new Padding(4, 5, 4, 5);
            bSendC2.Name = "bSendC2";
            bSendC2.Size = new Size(71, 40);
            bSendC2.TabIndex = 55;
            bSendC2.Text = "set";
            bSendC2.UseVisualStyleBackColor = true;
            bSendC2.Click += bSendC2_Click;
            // 
            // label9
            // 
            label9.Font = new Font("Times New Roman", 14F);
            label9.Location = new Point(-1, 340);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.MaximumSize = new Size(200, 33);
            label9.MinimumSize = new Size(240, 33);
            label9.Name = "label9";
            label9.Size = new Size(240, 33);
            label9.TabIndex = 54;
            label9.Text = "Anode, 2-8";
            label9.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtC2
            // 
            txtC2.Font = new Font("Times New Roman", 12F);
            txtC2.Location = new Point(18, 374);
            txtC2.Margin = new Padding(4, 5, 4, 5);
            txtC2.Name = "txtC2";
            txtC2.Size = new Size(72, 30);
            txtC2.TabIndex = 53;
            txtC2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // formsPlot1
            // 
            formsPlot1.DisplayScale = 1F;
            formsPlot1.Font = new Font("Times New Roman", 12F);
            formsPlot1.Location = new Point(510, 0);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(1390, 1000);
            formsPlot1.TabIndex = 57;
            // 
            // rbEMG2
            // 
            rbEMG2.AutoSize = true;
            rbEMG2.CheckAlign = ContentAlignment.BottomCenter;
            rbEMG2.Checked = true;
            rbEMG2.ImageAlign = ContentAlignment.TopCenter;
            rbEMG2.Location = new Point(5, 66);
            rbEMG2.Name = "rbEMG2";
            rbEMG2.Size = new Size(49, 40);
            rbEMG2.TabIndex = 67;
            rbEMG2.TabStop = true;
            rbEMG2.Text = "ECGR";
            rbEMG2.UseVisualStyleBackColor = true;
            rbEMG2.CheckedChanged += rbSignalMode_CheckedChange;
            // 
            // rbEMG1
            // 
            rbEMG1.AutoSize = true;
            rbEMG1.CheckAlign = ContentAlignment.BottomCenter;
            rbEMG1.Location = new Point(4, 14);
            rbEMG1.Name = "rbEMG1";
            rbEMG1.Size = new Size(51, 40);
            rbEMG1.TabIndex = 66;
            rbEMG1.Text = "ECGH";
            rbEMG1.UseVisualStyleBackColor = true;
            rbEMG1.CheckedChanged += rbSignalMode_CheckedChange;
            // 
            // txtFname
            // 
            txtFname.Font = new Font("Times New Roman", 14F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, 0);
            txtFname.Location = new Point(18, 174);
            txtFname.Margin = new Padding(4, 5, 4, 5);
            txtFname.Name = "txtFname";
            txtFname.Size = new Size(113, 34);
            txtFname.TabIndex = 65;
            txtFname.Text = "a1001";
            txtFname.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            txtFname.TextChanged += txtFname_TextChanged;
            // 
            // bSave
            // 
            bSave.Enabled = false;
            bSave.Font = new Font("Times New Roman", 14F);
            bSave.Location = new Point(13, 114);
            bSave.Margin = new Padding(0);
            bSave.Name = "bSave";
            bSave.Size = new Size(180, 50);
            bSave.TabIndex = 64;
            bSave.Text = "Start Saving";
            bSave.UseVisualStyleBackColor = true;
            bSave.Click += bSave_Click;
            // 
            // bView
            // 
            bView.Enabled = false;
            bView.Font = new Font("Times New Roman", 14F);
            bView.Location = new Point(13, 56);
            bView.Margin = new Padding(4, 5, 4, 5);
            bView.Name = "bView";
            bView.Size = new Size(180, 50);
            bView.TabIndex = 63;
            bView.Text = "Start Viewing";
            bView.UseVisualStyleBackColor = true;
            bView.Click += bView_Click;
            // 
            // rbEMG3
            // 
            rbEMG3.AutoSize = true;
            rbEMG3.CheckAlign = ContentAlignment.BottomCenter;
            rbEMG3.Location = new Point(3, 118);
            rbEMG3.Name = "rbEMG3";
            rbEMG3.Size = new Size(52, 40);
            rbEMG3.TabIndex = 69;
            rbEMG3.Text = "EMG1";
            rbEMG3.TextAlign = ContentAlignment.TopCenter;
            rbEMG3.UseVisualStyleBackColor = true;
            rbEMG3.CheckedChanged += rbSignalMode_CheckedChange;
            // 
            // rbEMG4
            // 
            rbEMG4.AutoSize = true;
            rbEMG4.CheckAlign = ContentAlignment.BottomCenter;
            rbEMG4.Location = new Point(3, 170);
            rbEMG4.Name = "rbEMG4";
            rbEMG4.Size = new Size(52, 40);
            rbEMG4.TabIndex = 70;
            rbEMG4.Text = "EMG2";
            rbEMG4.UseVisualStyleBackColor = true;
            rbEMG4.CheckedChanged += rbSignalMode_CheckedChange;
            // 
            // labelV
            // 
            labelV.Font = new Font("Times New Roman", 12F);
            labelV.Location = new Point(139, 178);
            labelV.Margin = new Padding(4, 0, 4, 0);
            labelV.MinimumSize = new Size(40, 34);
            labelV.Name = "labelV";
            labelV.Size = new Size(54, 34);
            labelV.TabIndex = 71;
            labelV.Text = "-";
            labelV.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // groupSignalMode
            // 
            groupSignalMode.Controls.Add(rbEMG3);
            groupSignalMode.Controls.Add(rbEMG1);
            groupSignalMode.Controls.Add(rbEMG4);
            groupSignalMode.Controls.Add(rbEMG2);
            groupSignalMode.Location = new Point(196, 0);
            groupSignalMode.Name = "groupSignalMode";
            groupSignalMode.Size = new Size(61, 217);
            groupSignalMode.TabIndex = 72;
            groupSignalMode.TabStop = false;
            // 
            // getImp
            // 
            getImp.Font = new Font("Times New Roman", 14F);
            getImp.Location = new Point(312, 60);
            getImp.Margin = new Padding(4, 5, 4, 5);
            getImp.Name = "getImp";
            getImp.Size = new Size(175, 42);
            getImp.TabIndex = 73;
            getImp.Text = "Get Impedance";
            getImp.TextAlign = ContentAlignment.BottomCenter;
            getImp.UseVisualStyleBackColor = true;
            getImp.Click += getImp_Click;
            // 
            // labelZ
            // 
            labelZ.Font = new Font("Times New Roman", 12F);
            labelZ.Location = new Point(347, 110);
            labelZ.Margin = new Padding(4, 0, 4, 0);
            labelZ.MinimumSize = new Size(40, 34);
            labelZ.Name = "labelZ";
            labelZ.Size = new Size(105, 40);
            labelZ.TabIndex = 74;
            labelZ.Text = "-";
            labelZ.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // groupSineWave
            // 
            groupSineWave.Controls.Add(bSetMaxBlock);
            groupSineWave.Controls.Add(txtMaxVNB);
            groupSineWave.Controls.Add(label17);
            groupSineWave.Controls.Add(bBlockMax);
            groupSineWave.Controls.Add(bGetV2);
            groupSineWave.Controls.Add(bSendV2);
            groupSineWave.Controls.Add(label10);
            groupSineWave.Controls.Add(txtV2);
            groupSineWave.Controls.Add(bGetV1);
            groupSineWave.Controls.Add(bSendV1);
            groupSineWave.Controls.Add(label11);
            groupSineWave.Controls.Add(txtV1);
            groupSineWave.Controls.Add(bGetVoff);
            groupSineWave.Controls.Add(bGetVon);
            groupSineWave.Controls.Add(bGetVfreq);
            groupSineWave.Controls.Add(bGetVamp);
            groupSineWave.Controls.Add(bSendVoff);
            groupSineWave.Controls.Add(txtVoff);
            groupSineWave.Controls.Add(label12);
            groupSineWave.Controls.Add(bSendVon);
            groupSineWave.Controls.Add(txtVon);
            groupSineWave.Controls.Add(label13);
            groupSineWave.Controls.Add(bSendVfreq);
            groupSineWave.Controls.Add(txtVfreq);
            groupSineWave.Controls.Add(label14);
            groupSineWave.Controls.Add(bSendVamp);
            groupSineWave.Controls.Add(label15);
            groupSineWave.Controls.Add(txtVAmp);
            groupSineWave.Enabled = false;
            groupSineWave.Location = new Point(257, 296);
            groupSineWave.Name = "groupSineWave";
            groupSineWave.Size = new Size(247, 559);
            groupSineWave.TabIndex = 73;
            groupSineWave.TabStop = false;
            // 
            // bSetMaxBlock
            // 
            bSetMaxBlock.Font = new Font("Times New Roman", 14F);
            bSetMaxBlock.Location = new Point(90, 513);
            bSetMaxBlock.Margin = new Padding(4, 5, 4, 5);
            bSetMaxBlock.Name = "bSetMaxBlock";
            bSetMaxBlock.Size = new Size(71, 40);
            bSetMaxBlock.TabIndex = 88;
            bSetMaxBlock.Text = "set";
            bSetMaxBlock.UseVisualStyleBackColor = true;
            bSetMaxBlock.Click += bSetMaxBlock_Click;
            // 
            // txtMaxVNB
            // 
            txtMaxVNB.Font = new Font("Times New Roman", 12F);
            txtMaxVNB.Location = new Point(10, 520);
            txtMaxVNB.Margin = new Padding(4, 5, 4, 5);
            txtMaxVNB.Name = "txtMaxVNB";
            txtMaxVNB.Size = new Size(72, 30);
            txtMaxVNB.TabIndex = 87;
            txtMaxVNB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label17
            // 
            label17.Font = new Font("Times New Roman", 14F);
            label17.Location = new Point(5, 479);
            label17.Margin = new Padding(4, 0, 4, 0);
            label17.MaximumSize = new Size(200, 33);
            label17.MinimumSize = new Size(240, 33);
            label17.Name = "label17";
            label17.Size = new Size(240, 33);
            label17.TabIndex = 86;
            label17.Text = "Max VNB";
            label17.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // bBlockMax
            // 
            bBlockMax.Font = new Font("Times New Roman", 14F);
            bBlockMax.Location = new Point(166, 516);
            bBlockMax.Margin = new Padding(4, 5, 4, 5);
            bBlockMax.Name = "bBlockMax";
            bBlockMax.Size = new Size(61, 37);
            bBlockMax.TabIndex = 85;
            bBlockMax.Text = "Get";
            bBlockMax.UseVisualStyleBackColor = true;
            bBlockMax.Click += bBlockMax_Click;
            // 
            // bGetV2
            // 
            bGetV2.Font = new Font("Times New Roman", 14F);
            bGetV2.Location = new Point(181, 114);
            bGetV2.Margin = new Padding(4, 5, 4, 5);
            bGetV2.Name = "bGetV2";
            bGetV2.Size = new Size(64, 40);
            bGetV2.TabIndex = 82;
            bGetV2.Text = "get";
            bGetV2.UseVisualStyleBackColor = true;
            bGetV2.Click += bGetV2_Click;
            // 
            // bSendV2
            // 
            bSendV2.Font = new Font("Times New Roman", 14F);
            bSendV2.Location = new Point(105, 114);
            bSendV2.Margin = new Padding(4, 5, 4, 5);
            bSendV2.Name = "bSendV2";
            bSendV2.Size = new Size(71, 40);
            bSendV2.TabIndex = 81;
            bSendV2.Text = "set";
            bSendV2.UseVisualStyleBackColor = true;
            bSendV2.Click += bSendV2_Click;
            // 
            // label10
            // 
            label10.Font = new Font("Times New Roman", 14F);
            label10.Location = new Point(6, 83);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.MaximumSize = new Size(200, 33);
            label10.MinimumSize = new Size(240, 33);
            label10.Name = "label10";
            label10.Size = new Size(240, 33);
            label10.TabIndex = 80;
            label10.Text = "Anode, 2-5";
            label10.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtV2
            // 
            txtV2.Font = new Font("Times New Roman", 12F);
            txtV2.Location = new Point(25, 117);
            txtV2.Margin = new Padding(4, 5, 4, 5);
            txtV2.Name = "txtV2";
            txtV2.Size = new Size(72, 30);
            txtV2.TabIndex = 79;
            txtV2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // bGetV1
            // 
            bGetV1.Font = new Font("Times New Roman", 14F);
            bGetV1.Location = new Point(181, 43);
            bGetV1.Margin = new Padding(4, 5, 4, 5);
            bGetV1.Name = "bGetV1";
            bGetV1.Size = new Size(64, 40);
            bGetV1.TabIndex = 78;
            bGetV1.Text = "get";
            bGetV1.UseVisualStyleBackColor = true;
            bGetV1.Click += bGetV1_Click;
            // 
            // bSendV1
            // 
            bSendV1.Font = new Font("Times New Roman", 14F);
            bSendV1.Location = new Point(105, 43);
            bSendV1.Margin = new Padding(4, 5, 4, 5);
            bSendV1.Name = "bSendV1";
            bSendV1.Size = new Size(71, 40);
            bSendV1.TabIndex = 77;
            bSendV1.Text = "set";
            bSendV1.UseVisualStyleBackColor = true;
            bSendV1.Click += bSendV1_Click;
            // 
            // label11
            // 
            label11.Font = new Font("Times New Roman", 14F);
            label11.Location = new Point(6, 12);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.MaximumSize = new Size(200, 33);
            label11.MinimumSize = new Size(240, 33);
            label11.Name = "label11";
            label11.Size = new Size(240, 33);
            label11.TabIndex = 76;
            label11.Text = "Cathode, 1-4";
            label11.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtV1
            // 
            txtV1.Font = new Font("Times New Roman", 12F);
            txtV1.Location = new Point(25, 46);
            txtV1.Margin = new Padding(4, 5, 4, 5);
            txtV1.Name = "txtV1";
            txtV1.Size = new Size(72, 30);
            txtV1.TabIndex = 75;
            txtV1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // bGetVoff
            // 
            bGetVoff.Font = new Font("Times New Roman", 14F);
            bGetVoff.Location = new Point(181, 437);
            bGetVoff.Margin = new Padding(4, 5, 4, 5);
            bGetVoff.Name = "bGetVoff";
            bGetVoff.Size = new Size(64, 40);
            bGetVoff.TabIndex = 74;
            bGetVoff.Text = "get";
            bGetVoff.UseVisualStyleBackColor = true;
            bGetVoff.Click += bGetVoff_Click;
            // 
            // bGetVon
            // 
            bGetVon.Font = new Font("Times New Roman", 14F);
            bGetVon.Location = new Point(181, 360);
            bGetVon.Margin = new Padding(4, 5, 4, 5);
            bGetVon.Name = "bGetVon";
            bGetVon.Size = new Size(64, 40);
            bGetVon.TabIndex = 73;
            bGetVon.Text = "get";
            bGetVon.UseVisualStyleBackColor = true;
            bGetVon.Click += bGetVon_Click;
            // 
            // bGetVfreq
            // 
            bGetVfreq.Font = new Font("Times New Roman", 14F);
            bGetVfreq.Location = new Point(181, 269);
            bGetVfreq.Margin = new Padding(4, 5, 4, 5);
            bGetVfreq.Name = "bGetVfreq";
            bGetVfreq.Size = new Size(64, 40);
            bGetVfreq.TabIndex = 72;
            bGetVfreq.Text = "get";
            bGetVfreq.UseVisualStyleBackColor = true;
            bGetVfreq.Click += bGetVfreq_Click;
            // 
            // bGetVamp
            // 
            bGetVamp.Font = new Font("Times New Roman", 14F);
            bGetVamp.Location = new Point(181, 186);
            bGetVamp.Margin = new Padding(4, 5, 4, 5);
            bGetVamp.Name = "bGetVamp";
            bGetVamp.Size = new Size(64, 40);
            bGetVamp.TabIndex = 71;
            bGetVamp.Text = "get";
            bGetVamp.UseVisualStyleBackColor = true;
            bGetVamp.Click += bGetVamp_Click;
            // 
            // bSendVoff
            // 
            bSendVoff.Font = new Font("Times New Roman", 14F);
            bSendVoff.Location = new Point(105, 437);
            bSendVoff.Margin = new Padding(4, 5, 4, 5);
            bSendVoff.Name = "bSendVoff";
            bSendVoff.Size = new Size(71, 40);
            bSendVoff.TabIndex = 70;
            bSendVoff.Text = "set";
            bSendVoff.UseVisualStyleBackColor = true;
            bSendVoff.Click += bSendVoff_Click;
            // 
            // txtVoff
            // 
            txtVoff.Font = new Font("Times New Roman", 12F);
            txtVoff.Location = new Point(25, 441);
            txtVoff.Margin = new Padding(4, 5, 4, 5);
            txtVoff.Name = "txtVoff";
            txtVoff.Size = new Size(72, 30);
            txtVoff.TabIndex = 69;
            txtVoff.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label12
            // 
            label12.Font = new Font("Times New Roman", 14F);
            label12.Location = new Point(6, 401);
            label12.Margin = new Padding(4, 0, 4, 0);
            label12.MaximumSize = new Size(200, 33);
            label12.MinimumSize = new Size(240, 33);
            label12.Name = "label12";
            label12.Size = new Size(240, 33);
            label12.TabIndex = 68;
            label12.Text = "Off Time, 0-300s";
            label12.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // bSendVon
            // 
            bSendVon.Font = new Font("Times New Roman", 14F);
            bSendVon.Location = new Point(105, 360);
            bSendVon.Margin = new Padding(4, 5, 4, 5);
            bSendVon.Name = "bSendVon";
            bSendVon.Size = new Size(71, 40);
            bSendVon.TabIndex = 67;
            bSendVon.Text = "set";
            bSendVon.UseVisualStyleBackColor = true;
            bSendVon.Click += bSendVon_Click;
            // 
            // txtVon
            // 
            txtVon.Font = new Font("Times New Roman", 12F);
            txtVon.Location = new Point(25, 363);
            txtVon.Margin = new Padding(4, 5, 4, 5);
            txtVon.Name = "txtVon";
            txtVon.Size = new Size(72, 30);
            txtVon.TabIndex = 66;
            txtVon.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label13
            // 
            label13.Font = new Font("Times New Roman", 14F);
            label13.Location = new Point(6, 325);
            label13.Margin = new Padding(4, 0, 4, 0);
            label13.MaximumSize = new Size(200, 33);
            label13.MinimumSize = new Size(240, 33);
            label13.Name = "label13";
            label13.Size = new Size(240, 33);
            label13.TabIndex = 65;
            label13.Text = "On time, 10-300s";
            label13.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // bSendVfreq
            // 
            bSendVfreq.Font = new Font("Times New Roman", 14F);
            bSendVfreq.Location = new Point(105, 269);
            bSendVfreq.Margin = new Padding(4, 5, 4, 5);
            bSendVfreq.Name = "bSendVfreq";
            bSendVfreq.Size = new Size(71, 40);
            bSendVfreq.TabIndex = 62;
            bSendVfreq.Text = "set";
            bSendVfreq.UseVisualStyleBackColor = true;
            bSendVfreq.Click += bSendVfreq_Click;
            // 
            // txtVfreq
            // 
            txtVfreq.Font = new Font("Times New Roman", 12F);
            txtVfreq.Location = new Point(25, 273);
            txtVfreq.Margin = new Padding(4, 5, 4, 5);
            txtVfreq.Name = "txtVfreq";
            txtVfreq.Size = new Size(72, 30);
            txtVfreq.TabIndex = 61;
            txtVfreq.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label14
            // 
            label14.Font = new Font("Times New Roman", 14F);
            label14.Location = new Point(6, 235);
            label14.Margin = new Padding(4, 0, 4, 0);
            label14.MaximumSize = new Size(200, 33);
            label14.MinimumSize = new Size(240, 33);
            label14.Name = "label14";
            label14.Size = new Size(240, 33);
            label14.TabIndex = 60;
            label14.Text = "Freq, 1-10 Hz";
            label14.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // bSendVamp
            // 
            bSendVamp.Font = new Font("Times New Roman", 14F);
            bSendVamp.Location = new Point(105, 186);
            bSendVamp.Margin = new Padding(4, 5, 4, 5);
            bSendVamp.Name = "bSendVamp";
            bSendVamp.Size = new Size(71, 40);
            bSendVamp.TabIndex = 59;
            bSendVamp.Text = "set";
            bSendVamp.UseVisualStyleBackColor = true;
            bSendVamp.Click += bSendVamp_Click;
            // 
            // label15
            // 
            label15.Font = new Font("Times New Roman", 14F);
            label15.Location = new Point(6, 155);
            label15.Margin = new Padding(4, 0, 4, 0);
            label15.MaximumSize = new Size(200, 33);
            label15.MinimumSize = new Size(240, 33);
            label15.Name = "label15";
            label15.Size = new Size(240, 33);
            label15.TabIndex = 58;
            label15.Text = "Amp (P-P), 0-0.2-5 mA";
            label15.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtVAmp
            // 
            txtVAmp.Font = new Font("Times New Roman", 12F);
            txtVAmp.Location = new Point(25, 189);
            txtVAmp.Margin = new Padding(4, 5, 4, 5);
            txtVAmp.Name = "txtVAmp";
            txtVAmp.Size = new Size(72, 30);
            txtVAmp.TabIndex = 57;
            txtVAmp.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // bStimMax
            // 
            bStimMax.Font = new Font("Times New Roman", 14F);
            bStimMax.Location = new Point(312, 935);
            bStimMax.Margin = new Padding(4, 5, 4, 5);
            bStimMax.Name = "bStimMax";
            bStimMax.Size = new Size(64, 79);
            bStimMax.TabIndex = 84;
            bStimMax.Text = "Max Stim";
            bStimMax.UseVisualStyleBackColor = true;
            bStimMax.Click += bStimMax_Click;
            // 
            // bZmin
            // 
            bZmin.Font = new Font("Times New Roman", 14F);
            bZmin.Location = new Point(401, 935);
            bZmin.Margin = new Padding(4, 5, 4, 5);
            bZmin.Name = "bZmin";
            bZmin.Size = new Size(81, 79);
            bZmin.TabIndex = 86;
            bZmin.Text = "Min Z";
            bZmin.UseVisualStyleBackColor = true;
            bZmin.Click += bZmin_Click;
            // 
            // ckVNB
            // 
            ckVNB.AutoSize = true;
            ckVNB.Location = new Point(318, 275);
            ckVNB.Name = "ckVNB";
            ckVNB.Size = new Size(134, 24);
            ckVNB.TabIndex = 87;
            ckVNB.Text = "Use Sine Wave?";
            ckVNB.UseVisualStyleBackColor = true;
            ckVNB.CheckedChanged += ckVNB_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1918, 1024);
            Controls.Add(ckVNB);
            Controls.Add(bZmin);
            Controls.Add(bStimMax);
            Controls.Add(groupSineWave);
            Controls.Add(labelZ);
            Controls.Add(getImp);
            Controls.Add(groupSignalMode);
            Controls.Add(labelV);
            Controls.Add(txtFname);
            Controls.Add(bSave);
            Controls.Add(bView);
            Controls.Add(bGetC2);
            Controls.Add(bSendC2);
            Controls.Add(label9);
            Controls.Add(txtC2);
            Controls.Add(bGetC1);
            Controls.Add(bSendC1);
            Controls.Add(label8);
            Controls.Add(txtC1);
            Controls.Add(bGetTF);
            Controls.Add(bGetTN);
            Controls.Add(bGetPR);
            Controls.Add(bGetPF);
            Controls.Add(bGetPW);
            Controls.Add(bGetPA);
            Controls.Add(bSendTF);
            Controls.Add(txtTF);
            Controls.Add(label7);
            Controls.Add(label5);
            Controls.Add(label6);
            Controls.Add(labelTX);
            Controls.Add(labelRX);
            Controls.Add(bGet);
            Controls.Add(bSendTN);
            Controls.Add(txtTN);
            Controls.Add(label3);
            Controls.Add(bSendPR);
            Controls.Add(txtPR);
            Controls.Add(label4);
            Controls.Add(bStimOff);
            Controls.Add(bStimOn);
            Controls.Add(bSendPF);
            Controls.Add(txtPF);
            Controls.Add(label2);
            Controls.Add(bSendPW);
            Controls.Add(txtPW);
            Controls.Add(label1);
            Controls.Add(bSendPA);
            Controls.Add(labelOpcode);
            Controls.Add(txtPA);
            Controls.Add(bDisconnect);
            Controls.Add(formsPlot1);
            Font = new Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(2);
            MaximizeBox = false;
            MdiChildrenMinimizedAnchorBottom = false;
            MinimizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "OpenNerve Controller";
            Load += Form1_Load;
            groupSignalMode.ResumeLayout(false);
            groupSignalMode.PerformLayout();
            groupSineWave.ResumeLayout(false);
            groupSineWave.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion
        public Button bDisconnect;
        public TextBox txtPA;
        public System.Windows.Forms.Label labelOpcode;
        public Button bSendPA;
        public System.Windows.Forms.Label label1;
        public Button bSendPW;
        public TextBox txtPW;
        public Button bSendPF;
        public TextBox txtPF;
        public System.Windows.Forms.Label label2;
        public Button bStimOff;
        public Button bStimOn;
        public Button bSendTN;
        public TextBox txtTN;
        public System.Windows.Forms.Label label3;
        public Button bSendPR;
        public TextBox txtPR;
        public System.Windows.Forms.Label label4;
        public Button bGet;
        public System.Windows.Forms.Label labelRX;
        public System.Windows.Forms.Label labelTX;
        public System.Windows.Forms.Label label5;
        public System.Windows.Forms.Label label6;
        public Button bSendTF;
        public TextBox txtTF;
        public System.Windows.Forms.Label label7;
        public Button bGetTF;
        public Button bGetTN;
        public Button bGetPR;
        public Button bGetPF;
        public Button bGetPW;
        public Button bGetPA;
        public Button bGetC1;
        public Button bSendC1;
        public System.Windows.Forms.Label label8;
        public TextBox txtC1;
        public Button bGetC2;
        public Button bSendC2;
        public System.Windows.Forms.Label label9;
        public TextBox txtC2;
        public ScottPlot.WinForms.FormsPlot formsPlot1;
        public Switch mySwitch;
        private RadioButton rbEMG2;
        private RadioButton rbEMG1;
        public TextBox txtFname;
        public Button bSave;
        public Button bView;
        private RadioButton rbEMG3;
        private RadioButton rbEMG4;
        public System.Windows.Forms.Label labelV;
        private GroupBox groupSignalMode;
        public Button getImp;
        public System.Windows.Forms.Label labelZ;
        private GroupBox groupSineWave;
        public Button bGetV2;
        public Button bSendV2;
        public System.Windows.Forms.Label label10;
        public TextBox txtV2;
        public Button bGetV1;
        public Button bSendV1;
        public System.Windows.Forms.Label label11;
        public TextBox txtV1;
        public Button bGetVoff;
        public Button bGetVon;
        public Button bGetVfreq;
        public Button bGetVamp;
        public Button bSendVoff;
        public TextBox txtVoff;
        public System.Windows.Forms.Label label12;
        public Button bSendVon;
        public TextBox txtVon;
        public System.Windows.Forms.Label label13;
        public Button bSendVfreq;
        public TextBox txtVfreq;
        public System.Windows.Forms.Label label14;
        public Button bSendVamp;
        public System.Windows.Forms.Label label15;
        public TextBox txtVAmp;
        public Button bStimMax;
        public Button bBlockMax;
        public Button bZmin;
        public Button bSetMaxBlock;
        public TextBox txtMaxVNB;
        public System.Windows.Forms.Label label17;
        private CheckBox ckVNB;
    }
}

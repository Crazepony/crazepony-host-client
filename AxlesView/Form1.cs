using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using AxlesView.Properties;

using OpenTK;
using OpenTK.Graphics;

namespace AxlesView
{
    public enum DataMode { Text, Hex,ShowHex}
    public enum LogMsgType { Incoming, Outgoing, Normal, Warning, Error };
	public partial class Form1 : Form
	{
        #region Local Variables

        // The main control for communicating through the RS-232 port
        private SerialPort comport = new SerialPort();

        // Various colors for logging info
        private Color[] LogMsgTypeColor = { Color.Blue, Color.Green, Color.Black, Color.Orange, Color.Red };

        // Temp holder for whether a key was pressed
        private bool KeyHandled = false;

        #endregion
		public Form1()
		{
            // Build the form
            InitializeComponent();

            // Restore the users settings
            InitializeControlValues();

            // Enable/disable controls based on the current state
            EnableControls();

            // When data is recieved through the port, call this method
            comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

		}
        /// <summary> Save the user's settings. </summary>
     
        private void InitializeControlValues()
        {
            cmbParity.Items.Clear();
            cmbParity.Items.AddRange(Enum.GetNames(typeof(Parity)));
  
       
            cmbPortName.Items.Clear();
            cmbParity.Text = "None";
            
            cmbDataBits.Text = "8";
            foreach (string s in SerialPort.GetPortNames())
                cmbPortName.Items.Add(s);

            if (cmbPortName.Items.Contains(Settings.Default.PortName))
                cmbPortName.Text = Settings.Default.PortName;
            else if (cmbPortName.Items.Count > 0)
                cmbPortName.SelectedIndex = 0;
            else
            {
                MessageBox.Show(this, "There are no COM Ports detected on this computer.\nPlease install a COM Port and restart this app.", "No COM Ports Installed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
        /// <summary> Enable/disable controls based on the app's current state. </summary>
    private void EnableControls()
    {
      // Enable/disable controls based on whether the port is open or not
      gbPortSettings.Enabled = !comport.IsOpen;
      txtSendData.Enabled = btnSend.Enabled = comport.IsOpen;

      if (comport.IsOpen) 
          btnOpenPort.Text = "&Close Port";
      else 
          btnOpenPort.Text = "&Open Port";
    }
    private void GetPID()
    {
        byte[] data = { 0xAA, 0xCC,0, 0, 50, 50, 50, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        if (comport.IsOpen)
        {
            // Send the binary data out the port
            comport.Write(data, 0,32);
            comport.DiscardOutBuffer();
                
            // Show the hex digits on in the terminal window
            Log(LogMsgType.Outgoing, "【发送】读取PID参数...\n");
        }
        else Log(LogMsgType.Outgoing, "【错误】请先连接串口！\n");
    }

    /// <summary> Send the user's data currently entered in the 'send' box.</summary>
    private void SendData()
    {
      if (CurrentDataMode == DataMode.Text)
      {
        // Send the user's text straight out the port
        comport.Write(txtSendData.Text);

        // Show in the terminal window the user's text
        Log(LogMsgType.Outgoing, "【发送】传送数据:" + txtSendData.Text + "\n");
      }
      else
      {
        try
        {
          // Convert the user's string of hex digits (ex: B4 CA E2) to a byte array
          byte[] data = HexStringToByteArray(txtSendData.Text);

          // Send the binary data out the port
          comport.Write(data, 0, data.Length);

          // Show the hex digits on in the terminal window
          Log(LogMsgType.Outgoing, "【发送】传送数据:"+ByteArrayToHexString(data) + "\n");
        }
        catch (FormatException)
        {
          // Inform the user if the hex string was not properly formatted
          Log(LogMsgType.Error, "Not properly formatted hex string: " + txtSendData.Text + "\n");
        }
      }
      txtSendData.SelectAll();
    }

    /// <summary> Log data to the terminal window. </summary>
    /// <param name="msgtype"> The type of message to be written. </param>
    /// <param name="msg"> The string containing the message to be shown. </param>
    private void Log(LogMsgType msgtype, string msg)
    {
      rtfTerminal.Invoke(new EventHandler(delegate
      {
        rtfTerminal.SelectedText = string.Empty;
        rtfTerminal.SelectionFont = new Font(rtfTerminal.SelectionFont, FontStyle.Bold);
        rtfTerminal.SelectionColor = LogMsgTypeColor[(int)msgtype];
        rtfTerminal.AppendText(msg);
        rtfTerminal.ScrollToCaret();
      }));
    }
    private void SetMotor1(int value)
    {
        Motor1.Invoke(new EventHandler(delegate
        {
            Motor1.Value=value;
        }));
    }
    private void SetMotor2(int value)
    {
        Motor1.Invoke(new EventHandler(delegate
        {
            Motor2.Value = value;
        }));
    }
    private void SetMotor3(int value)
    {
        Motor1.Invoke(new EventHandler(delegate
        {
            Motor3.Value = value;
        }));
    }
    private void SetMotor4(int value)
    {
        Motor4.Invoke(new EventHandler(delegate
        {
            Motor4.Value = value;
        }));
    }
    private void Rotate3D_X(int value)
    {
        axles3D1.Invoke(new EventHandler(delegate
        {
            axles3D1.AngleX = value;
        }));
    }
    private void Rotate3D_Y(int value)
    {
        axles3D1.Invoke(new EventHandler(delegate
        {
            axles3D1.AngleY = value;
        }));
    }
    private void Rotate3D_Z(int value)
    {
        axles3D1.Invoke(new EventHandler(delegate
        {
            axles3D1.AngleZ = value;
        }));
    }
    private void ShowRow(string msg)
    {
        row_label.Invoke(new EventHandler(delegate
        {
            row_label.Text = msg;
        }));
    }

    private void ShowPit(string msg)
    {
        row_label.Invoke(new EventHandler(delegate
        {
            pit_label.Text = msg;
        }));
    }
    private void ShowYaw(string msg)
    {
        row_label.Invoke(new EventHandler(delegate
        {
            yaw_label.Text = msg;
        }));
    }
 
    

    /// <summary> Convert a string of hex digits (ex: E4 CA B2) to a byte array. </summary>
    /// <param name="s"> The string containing the hex digits (with or without spaces). </param>
    /// <returns> Returns an array of bytes. </returns>
    private byte[] HexStringToByteArray(string s)
    {
      s = s.Replace(" ", "");
      byte[] buffer = new byte[s.Length / 2];
      for (int i = 0; i < s.Length; i += 2)
        buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
      return buffer;
    }

    /// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
    /// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
    /// <returns> Returns a well formatted string of hex digits with spacing. </returns>
    private string ByteArrayToHexString(byte[] data)
    {
      StringBuilder sb = new StringBuilder(data.Length * 3);
      foreach (byte b in data)
        sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
      return sb.ToString().ToUpper();
    }

    #region Local Properties
    private DataMode CurrentDataMode
    {
      get
      {
          if (rbHex.Checked)
              return DataMode.Hex;
          else if (rbhexshow.Checked)
              return DataMode.ShowHex;
          else
              return DataMode.Text;
      }
      set
      {
          if (value == DataMode.Text)
              rbText.Checked = true;
          else if (value == DataMode.ShowHex) rbhexshow.Checked = true;
          else rbHex.Checked = true;
      }
    }
    #endregion

    #region Event Handlers
   
    private void frmTerminal_Shown(object sender, EventArgs e)
    {
      Log(LogMsgType.Normal, String.Format("Application Started at {0}\n", DateTime.Now));
    }
    private void frmTerminal_FormClosing(object sender, FormClosingEventArgs e)
    {
      // The form is closing, save the user's preferences
      SaveSettings();
    }
    private void SaveSettings()
    {
        Settings.Default.BaudRate = int.Parse(cmbBaudRate.Text);
        Settings.Default.DataBits = int.Parse(cmbDataBits.Text);
       // Settings.Default.DataMode = CurrentDataMode;
        Settings.Default.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);
    //    Settings.Default.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBits.Text);
        Settings.Default.PortName = cmbPortName.Text;
      

        Settings.Default.Save();
      
    }
    private void rbText_CheckedChanged(object sender, EventArgs e)
    { 
        if (rbText.Checked) 
            CurrentDataMode = DataMode.Text; 
    }
    private void rbHex_CheckedChanged(object sender, EventArgs e)
    {
        if (rbHex.Checked) 
            CurrentDataMode = DataMode.Hex; 
    }
    private void cmbBaudRate_Validating(object sender, CancelEventArgs e)
    { 
        int x; 
        e.Cancel = !int.TryParse(cmbBaudRate.Text, out x); 
    }
    private void cmbDataBits_Validating(object sender, CancelEventArgs e)
    { 
        int x; 
        e.Cancel = !int.TryParse(cmbDataBits.Text, out x); 
    }
    private void btnOpenPort_Click(object sender, EventArgs e)
    {
      // If the port is open, close it.
      if (comport.IsOpen) 
          comport.Close();
      else
      {
        // Set the port's settings
        comport.BaudRate = int.Parse(cmbBaudRate.Text);
        comport.DataBits = int.Parse(cmbDataBits.Text);
 //       comport.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBits.Text);
        comport.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);
        comport.PortName = cmbPortName.Text;

        // Open the port
        comport.Open();
      }

      // Change the state of the form's controls
      EnableControls();

      // If the port is open, send focus to the send data box
      if (comport.IsOpen) 
          txtSendData.Focus();
    }
    private void btnSend_Click(object sender, EventArgs e)
    { 
        SendData(); 
    }
    private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      // This method will be called when there is data waiting in the port's buffer
        int i;
        int temp;
        Byte sum=0;
      // Determain which mode (string or binary) the user is in
      if (CurrentDataMode == DataMode.Text)
      {
        // Read all the data waiting in the buffer
        string data = comport.ReadExisting();

        // Display the text to the user in the terminal
        Log(LogMsgType.Incoming, data);
      }
      else if (CurrentDataMode == DataMode.Hex)
      {
          // Obtain the number of bytes waiting in the port's buffer
          int bytes = 60;//comport.BytesToRead;

          // Create a byte array buffer to hold the incoming data
          byte[] buffer = new byte[bytes];


          // Read the data from the port and store it in our buffer
          comport.Read(buffer, 0, bytes);
       
          for (i = 0; i < 20; i++)
          {
              //判断协议头
              if ((buffer[i] == 0x88) && (buffer[i + 1] == 0xAF) && (buffer[i + 2] == 0x1C))
              {
                  //校验加和
                  for (int j = i + 3; j < i + 17; j++)
                  {
                      sum += buffer[j];

                  }
                  //检查校验和正确
                  if (sum == buffer[i + 17] && sum != 0)
                  {
                      //减500的原因是在飞机里数据加了500把负数转换成正数
                      temp = ((buffer[i + 3] << 8) + buffer[i + 4]) - 500;

                      ShowRow(temp.ToString());
                      Rotate3D_X(temp);

                      temp = ((buffer[i + 5] << 8) + buffer[i + 6]) - 500;
                      ShowPit(temp.ToString());
                      Rotate3D_Z(-temp);
                    //没显示YAW轴，需要的话解注释：
                    //  temp = ((buffer[i + 7] << 8) + buffer[i + 8]) - 180;
                      //  ShowYaw(temp.ToString());
                    ///  Rotate3D_Y(-temp);


                      //更新电机状态
                      SetMotor1((buffer[i + 9] << 8) + buffer[i + 10]);
                      SetMotor2((buffer[i + 11] << 8) + buffer[i + 12]);
                      SetMotor3((buffer[i + 13] << 8) + buffer[i + 14]);
                      SetMotor4((buffer[i + 15] << 8) + buffer[i + 16]);




                  }
                  sum = 0;
              }

          }

          // Show the user the incoming data in hex format
          //Log(LogMsgType.Incoming, ByteArrayToHexString(buffer));
      }
      else 
      {
          // Obtain the number of bytes waiting in the port's buffer
          int bytes = 80;//comport.BytesToRead;

          // Create a byte array buffer to hold the incoming data
          byte[] buffer = new byte[bytes];


          // Read the data from the port and store it in our buffer
          comport.Read(buffer, 0, bytes);
          // Show the user the incoming data in hex format
          Log(LogMsgType.Incoming, ByteArrayToHexString(buffer));
      }
    }
    private void txtSendData_KeyDown(object sender, KeyEventArgs e)
    { 
      // If the user presses [ENTER], send the data now
      if (KeyHandled = e.KeyCode == Keys.Enter) { e.Handled = true; SendData(); } 
    }
    private void txtSendData_KeyPress(object sender, KeyPressEventArgs e)
    { 
        e.Handled = KeyHandled; 
    }
    #endregion

	

      

        private void btnOpenPort_Click_1(object sender, EventArgs e)
        {
            // If the port is open, close it.
            if (comport.IsOpen)
            {
              comport.Close();
            }
            else
            {
                // Set the port's settings
                comport.BaudRate = int.Parse(cmbBaudRate.Text);
                comport.DataBits = int.Parse(cmbDataBits.Text);
                //不设置停止位了，用不上      comport.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBits.Text);
                comport.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);
                comport.PortName = cmbPortName.Text;

                // Open the port
                comport.Open();
            }

            // Change the state of the form's controls
            EnableControls();

            // If the port is open, send focus to the send data box
            if (comport.IsOpen)
                txtSendData.Focus();
        }

        private void btnSend_Click_1(object sender, EventArgs e)
        {
            SendData(); 
        }

    

        private void throttle_Scroll(object sender, EventArgs e)
        {
            //待发送的数据：
            byte[] data = { 0xAA, 0xBB,0,0,50,50,50,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0xA5 };
           if (comport.IsOpen)
           {
               if (throttle.Value > 5)
               {
                   data[2] = (byte)(throttle.Value * 10 & 0x00ff);
                   data[3] = (byte)((throttle.Value * 10) >> 8);
                   data[31] = 0xA5;
               }
               else data[31] = 0x00;
               // Send the binary data out the port
                comport.Write(data, 0, 32);
                comport.DiscardOutBuffer();
               // Show the hex digits on in the terminal window
                Log(LogMsgType.Outgoing, "【遥控】油门控制当前值为:"+throttle.Value*10+"\n");
              
           }
           else Log(LogMsgType.Outgoing, "【错误】请先连接串口！\n");
        }

        private void cmbBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cmbDataBits_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void rtfTerminal_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void cmbParity_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void txtSendData_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            rtfTerminal.Text = "";
        }
       
       

        private void axles3D1_Load(object sender, EventArgs e)
        {

        }

      

     
      
      

     
	}
}
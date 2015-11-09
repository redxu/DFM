/*
 * Created by SharpDevelop.
 * User: RedXu
 * Date: 2015-11-02
 * Time: 11:05
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;

namespace DFM
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		int count = 0;
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void Btn_makeClick(object sender, EventArgs e)
		{
			EnvInit();
			Font font = new Font("Vrinda",17);
			string s = this.textBox1.Text;
			string[] sarry = s.Split("\r\n".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
			foreach(string str in sarry)
			{
				byte[] dots = String2Dot(str.Trim(),font);
				SaveBin(dots);
			}
		}
		
		void EnvInit()
		{
			File.Delete(@"./dot12_24.bin");
			File.Delete(@"./dot12_24.txt");
			count = 0;
		}
		
		void SaveBin(Byte[] data)
		{
			StreamWriter sw = new StreamWriter(@"./dot12_24.bin",true);
			BinaryWriter bw = new BinaryWriter(sw.BaseStream);
			bw.Write(data);
			bw.Flush();
			sw.Close();
			bw.Close();
		}
		
		void SaveOffset(string str,int length)
		{
			int end = count + (length/12) - 1;
			StreamWriter sw = new StreamWriter(@"./dot12_24.txt",true,System.Text.Encoding.UTF8);
			String s = String.Format("{0} [{1}-{2}]",str,count,end);
			sw.WriteLine(s);
			sw.Close();
			count += (length/12);
		}
		
		Byte[] String2Dot(string str,Font font)
		{
			int expwidth;
			Bitmap bm = new Bitmap(800,24);
			Graphics g = Graphics.FromImage((Image)bm);
			g.FillRectangle(Brushes.White,0,0,bm.Width,bm.Height);
			g.DrawString(str,font,Brushes.Black,0,-1);
			g.Save();
			g.Dispose();
			
			//try gray
			int w = bm.Width;
			int h = bm.Height;
			for(int i=0;i<w;i++)
			{
				for(int j=0;j<h;j++)
				{
					Color c = bm.GetPixel(i,j);
					if(c.GetBrightness() > 0.5)
					{
						bm.SetPixel(i,j,Color.White);
					}
					else
					{
						bm.SetPixel(i,j,Color.Black);
					}
				}
			}
			
			//auto cut.
			int startx,endx;
			startx = endx = -1;
			for(int i=0;i<w;i++)
			{
				for(int j=0;j<h;j++)
				{
					Color c = bm.GetPixel(i,j);
					if(c.R == 0)
					{
						if(startx == -1)
						{
							startx = i;
						}
						
						endx = i;
					}
				}
			}
			
			expwidth = (endx-startx+1);
			expwidth = ((expwidth+11)/12)*12;
			Trace.WriteLine(string.Format("start x {0} end x {1} expwidth {2}",startx,endx,expwidth));
			
			Bitmap bitmap = bm.Clone(new Rectangle(startx,0,expwidth,bm.Height),bm.PixelFormat);
			this.pictureBox1.Image = (Image)bitmap.Clone();
			//save offset
			SaveOffset(str,expwidth);
			
			//to bits.
			w = bitmap.Width;
			h = bitmap.Height;
			
			Byte[,] bits = new byte[w,h];
			bits.Initialize();
			for(int i=0;i<w;i++)
			{
				for(int j=0;j<h;j++)
				{
					Color c = bitmap.GetPixel(i,j);
					if(c.R >= 250 && c.G >= 250 && c.B >= 250)
					{
						bits[i,j] = 0;
					}
					else
					{
						bits[i,j] = 1;
					}
				}
			}
			
			bitmap.Save(str+".png");
			bitmap.Dispose();
			
			//auto generote code.
			//up->down then left->right little ending
			Byte[] code = new Byte[w*(h/8)];
			int codeindex = 0;
			int tmpcode = 0;
			for(int i=0;i<w;i++)
			{
				for(int j=0;j<(h/8);j++)
				{
					tmpcode = bits[i,j*8] << 7;
					tmpcode |= bits[i,j*8+1] << 6;
					tmpcode |= bits[i,j*8+2] << 5;
					tmpcode |= bits[i,j*8+3] << 4;
					tmpcode |= bits[i,j*8+4] << 3;
					tmpcode |= bits[i,j*8+5] << 2;
					tmpcode |= bits[i,j*8+6] << 1;
					tmpcode |= bits[i,j*8+7];
					
					code[codeindex++] = (Byte)tmpcode;
					Trace.Write(string.Format("0x{0}, ",tmpcode.ToString("X2")));

					if((codeindex) % 12 == 0)
					{
						Trace.WriteLine("");
					}
				}
			}
			
			return code;
		}
		
		/// <summary>
		/// 美工修图后
		/// </summary>
		/// <param name="str"></param>
		/// <param name="font"></param>
		/// <returns></returns>
		Byte[] Image2Dot(string str,Font font)
		{
			Bitmap bitmap = (Bitmap)Bitmap.FromFile(str+".png");
			//save offset
			SaveOffset(str,bitmap.Width);
			
			//to bits.
			int w = bitmap.Width;
			int h = bitmap.Height;
			
			Byte[,] bits = new byte[w,h];
			bits.Initialize();
			for(int i=0;i<w;i++)
			{
				for(int j=0;j<h;j++)
				{
					Color c = bitmap.GetPixel(i,j);
					if(c.R >= 250 && c.G >= 250 && c.B >= 250)
					{
						bits[i,j] = 0;
					}
					else
					{
						bits[i,j] = 1;
					}
				}
			}
			
			bitmap.Dispose();
			
			//auto generote code.
			//up->down then left->right little ending
			Byte[] code = new Byte[w*(h/8)];
			int codeindex = 0;
			int tmpcode = 0;
			for(int i=0;i<w;i++)
			{
				for(int j=0;j<(h/8);j++)
				{
					tmpcode = bits[i,j*8] << 7;
					tmpcode |= bits[i,j*8+1] << 6;
					tmpcode |= bits[i,j*8+2] << 5;
					tmpcode |= bits[i,j*8+3] << 4;
					tmpcode |= bits[i,j*8+4] << 3;
					tmpcode |= bits[i,j*8+5] << 2;
					tmpcode |= bits[i,j*8+6] << 1;
					tmpcode |= bits[i,j*8+7];
					
					code[codeindex++] = (Byte)tmpcode;
					Trace.Write(string.Format("0x{0}, ",tmpcode.ToString("X2")));

					if((codeindex) % 12 == 0)
					{
						Trace.WriteLine("");
					}
				}
			}
			
			return code;			
		}
		
		void Btn_fixClick(object sender, EventArgs e)
		{
			EnvInit();
			Font font = new Font("Vrinda",17);
			string s = this.textBox1.Text;
			string[] sarry = s.Split("\r\n".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
			foreach(string str in sarry)
			{
				byte[] dots = Image2Dot(str.Trim(),font);
				SaveBin(dots);
			}			
		}
	}
}

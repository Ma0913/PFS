using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace MiniProject
{
    public partial class DashBoard : Form
    {
        //게이지 이미지 Path
        private string gaugePath = @"C:\Users\master\Desktop\MiniProj\resource\guage\";
        private string RPMNeedleImg = "RPMNeedle.png";
        private string speedNeedleImg = "SpeedNeedle.png";
        //타이머 이미지 Path
        private string digitalNumberPath = @"C:\Users\master\Desktop\MiniProj\resource\No\";
        private string digitalUpImg = "digitalUp.png";
        private string digitalDownImg = "digitalDown.png";
        //전원스위치 이미지 Path
        private string switchPath = @"C:\Users\master\Desktop\MiniProj\resource\switch\";
        private string switchOnImg = "switchON.png";
        private string switchOffImg = "switchOFF.png";
        //전원 상태
        private bool powerState = false;
        //레버
        private bool isDragging = false;
        private int yOffset;
        private int speedValue = 0;
        private int speedLevel = 0;
        private float previousRate = 0.25f;
        private Timer leverAnimationTimer;
        //게이지
        private int rectangleX;
        private int rectangleY = 0;
        private int rectangleWidth = 138;
        private int rectangleHeight = 255;

        //Task 종료용
        private bool TaskBreak = true;

        public DashBoard()
        {
            InitializeComponent();
            InitializeTimerDefaultImage();
            InitializeAnimation();

            AddBorderTopAndBottom(step1);
            AddBorderTopAndBottom(step2);
            AddBorderTopAndBottom(step3);

#pragma warning disable CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
            TaskUpdateImg();
#pragma warning restore CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.

            //TextBox 출력
            speedTextLabel.Text = speedValue.ToString();
            levelTextLabel.Text = speedLevel.ToString();

            //leverLocationChange 이벤트 추가
            lever.LocationChanged += Lever_LocationChanged;

            FormClosing += DashBoard_FormClosing;
        }

        //*****
        //전원 스위치
        //*****
        private void PowerPictureBox_Click_DashBoard(object sender, EventArgs e)
        {
            powerState = !powerState;
            UpdatePowerImage();

            if (powerState)
            {
                Background.Instance.Video_Play();
                Background.Instance.Video_Rate(previousRate);
            }
        }
        private void UpdatePowerImage()
        {
            string switchImgPath = switchPath + (powerState ? switchOnImg : switchOffImg);

            try
            {
                powerPictureBox.BackgroundImage = Image.FromFile(switchImgPath);

                if (powerState)
                {
                    //전원 ON
                    lever.MouseDown += Lever_MouseDown;
                    lever.MouseUp += Lever_MouseUp;
                    lever.MouseMove += Lever_MouseMove;
                }
                else
                {
                    //전원 OFF
                    lever.MouseDown -= Lever_MouseDown;
                    lever.MouseUp -= Lever_MouseUp;
                    lever.MouseMove -= Lever_MouseMove;

                    PowerOffLeverAnimation();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("powerSwitch Exception : " + ex.Message);
            }
        }

        //*****
        //레버
        //*****
        // 레버 위치 변경 애니메이션
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // 애니메이션을 시작할 때 목표 Y 좌표 설정
            int targetY = leverBackground.Height - lever.Height;
            int animationSpeed = 5;
            int currentY = lever.Location.Y;
            if (currentY < targetY)
            {
                currentY += animationSpeed;
                if (currentY >= targetY)
                {
                    currentY = targetY;
                }
            }
            else if (currentY > targetY)
            {
                currentY -= animationSpeed;
                if (currentY <= targetY)
                {
                    currentY = targetY;
                }
            }
            lever.Location = new Point(lever.Location.X, currentY);

            // 목표에 도달하면 애니메이션 중지
            if (currentY == targetY)
            {
                leverAnimationTimer.Stop();
                Background.Instance.Video_Stop();
            }
        }
        private void InitializeAnimation()
        {
            leverAnimationTimer = new Timer();
            leverAnimationTimer.Start();
            //타이머 간격(40ms)
            leverAnimationTimer.Interval = 40;
            leverAnimationTimer.Tick += AnimationTimer_Tick;
        }
        private void PowerOffLeverAnimation()
        {
            try
            {
                leverAnimationTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("PowerOffLeverAnimation Exception : " + ex.Message);
            }
        }
        //stepPanel AddBorder
        private void AddBorderTopAndBottom(Panel panel)
        {
            panel.Paint += (sender, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle,
                    Color.Black, 0, ButtonBorderStyle.None,
                    Color.Black, 3, ButtonBorderStyle.Solid,
                    Color.Black, 0, ButtonBorderStyle.None,
                    Color.Black, 3, ButtonBorderStyle.Solid);
            };
        }

        //레버 마우스 이벤트
        private void Lever_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                yOffset = e.Y;
            }
        }
        private void Lever_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int newY = lever.Top + e.Y - yOffset;

                int minY = 0;
                int maxY = leverBackground.Height - lever.Height;

                // newY를 minY와 maxY 범위 내로 제한
                newY = Math.Max(minY, Math.Min(newY, maxY));

                lever.Top = newY;
            }
        }
        private void Lever_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            yOffset = e.Y;
        }
        private void Lever_LocationChanged(object sender, EventArgs e)
        {
            speedValue = 200 - lever.Location.Y;
            speedLevel = (speedValue == 0) ? 0 : ((speedValue - 1) / 40 + 1);

            speedTextLabel.Text = speedValue.ToString();
            levelTextLabel.Text = speedLevel.ToString();

            float rate = speedLevel * 0.25f;
            if (rate != previousRate)
            {
                Background.Instance.Video_Rate(rate);
                previousRate = rate;
            }
        }

        //*****
        //게이지&타이머
        //***** 
        private async Task TaskUpdateImg()
        {
            Image timerUpImg = Image.FromFile(digitalNumberPath + digitalUpImg);
            Image timerDownImg = Image.FromFile(digitalNumberPath + digitalDownImg);

            Image RPMImg = Image.FromFile(gaugePath + RPMNeedleImg);
            Image speedImg = Image.FromFile(gaugePath + speedNeedleImg);

            RPMPictureBox.BackgroundImage = RPMImg;
            speedPictureBox.BackgroundImage = speedImg;

            while (TaskBreak)
            {
                //100밀리세컨 딜레이
                await Task.Delay(100);

                //업로드
                this.Invoke((MethodInvoker)delegate
                {
                    // 타이머 이미지
                    RotateTimerImage();

                    // 게이지 이미지
                    RPMPictureBox.BackgroundImage = RotateImage(RPMImg, RPMAngle(speedValue));
                    speedPictureBox.BackgroundImage = RotateImage(speedImg, SpeedAngle(speedValue));
                });

                if (!TaskBreak)
                {
                    Debug.WriteLine("break");
                    break;
                }
            }
        }
        //타이머
        //최초 타이머 이미지
        private void TimeImageDefault(PictureBox boxName)
        {
            try
            {
                Image timerDownImg = Image.FromFile(digitalNumberPath + digitalDownImg);

                rectangleX = (boxName.Name.Contains("colon")) ? 1505 : 1290;

                Bitmap timerBitMap = new Bitmap(rectangleWidth, rectangleHeight);
                using (Graphics g = Graphics.FromImage(timerBitMap))
                {
                    g.DrawImage(timerDownImg, new Rectangle(0, 0, rectangleWidth, rectangleHeight),
                        new Rectangle(rectangleX, rectangleY, rectangleWidth, rectangleHeight), GraphicsUnit.Pixel);
                }
                boxName.BackgroundImage = timerBitMap;
            }
            catch (Exception ex)
            {
                MessageBox.Show("TimeImageDefault Exception : " + ex.Message);
            }
        }
        //전환 타이머 이미지
        private void TimeImageChange(int number, PictureBox boxName)
        {
            try
            {
                string digitalNumPath = null;
                if (number >= 0 && number < 6)
                {
                    digitalNumPath = digitalNumberPath + digitalUpImg;
                }
                else if (number >= 6 && number <= 10)
                {
                    digitalNumPath = digitalNumberPath + digitalDownImg;
                }

                switch (number)
                {
                    case 6: rectangleX = 0; break;
                    case 7: rectangleX = 215; break;
                    case 0:
                    case 8: rectangleX = 430; break;
                    case 1:
                    case 9: rectangleX = 645; break;
                    case 2: rectangleX = 860; break;
                    case 3: rectangleX = 1075; break;
                    case 4: rectangleX = 1290; break;
                    case 5: rectangleX = 1505; break;
                    default: break;
                }

                Image timerImg = Image.FromFile(digitalNumPath);
                Bitmap timerBitMap = new Bitmap(rectangleWidth, rectangleHeight);
                using (Graphics g = Graphics.FromImage(timerBitMap))
                {
                    g.DrawImage(timerImg, new Rectangle(0, 0, rectangleWidth, rectangleHeight),
                        new Rectangle(rectangleX, rectangleY, rectangleWidth, rectangleHeight), GraphicsUnit.Pixel);
                }

                boxName.BackgroundImage = timerBitMap;
            }
            catch (Exception ex)
            {
                MessageBox.Show("TimeImageChange Exception : " + ex.Message);
            }
        }
        private void RotateTimerImage()
        {
            DateTime now = DateTime.Now;

            int hour1 = now.Hour / 10;
            int hour2 = now.Hour % 10;
            int minute1 = now.Minute / 10;
            int minute2 = now.Minute % 10;
            int second1 = now.Second / 10;
            int second2 = now.Second % 10;

            if (powerState)
            {
                TimeImageChange(hour1, hourBox1);
                TimeImageChange(hour2, hourBox2);
                TimeImageChange(minute1, minuteBox1);
                TimeImageChange(minute2, minuteBox2);
                TimeImageChange(second1, secondBox1);
                TimeImageChange(second2, secondBox2);
            }
            else
            {
                TimeImageDefault(hourBox1);
                TimeImageDefault(hourBox2);
                TimeImageDefault(colonBox1);
                TimeImageDefault(minuteBox1);
                TimeImageDefault(minuteBox2);
                TimeImageDefault(colonBox2);
                TimeImageDefault(secondBox1);
                TimeImageDefault(secondBox2);
            }
        }
        private void InitializeTimerDefaultImage()
        {
            TimeImageDefault(hourBox1);
            TimeImageDefault(hourBox2);
            TimeImageDefault(colonBox1);
            TimeImageDefault(minuteBox1);
            TimeImageDefault(minuteBox2);
            TimeImageDefault(colonBox2);
            TimeImageDefault(secondBox1);
            TimeImageDefault(secondBox2);
        }

        //게이지
        //회전각 return
        private double RPMAngle(int speedValue)
        {
            double rotateAngle = 0;
            double RPMDivision = 1.34;
            rotateAngle = (speedValue * RPMDivision);

            return rotateAngle;
        }
        private double SpeedAngle(int speedValue)
        {
            int defaultSpeedAngle = 0;
            double rotateAngle = 0;
            double speedDivision = 1.3825;

            defaultSpeedAngle = (powerState) ? 36 : 0;
            rotateAngle = defaultSpeedAngle + (speedValue * speedDivision);

            return rotateAngle;
        }
        //회전 이미지 return
        private Image RotateImage(Image image, double angle)
        {
            Bitmap rotatedImage = new Bitmap(image.Width, image.Height);

            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                // 중심을 원점으로 설정
                g.TranslateTransform(image.Width / 2, image.Height / 2);
                // 회전 적용
                g.RotateTransform((float)angle);
                // 다시 원래 위치로 이동
                g.TranslateTransform(-image.Width / 2, -image.Height / 2);
                g.DrawImage(image, new Point(0, 0));
            }
            return rotatedImage;
        }

        private void DashBoard_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 루프 종료를 위해 취소
            TaskBreak = false;
        }
    }
}

using LibVLCSharp.Shared;
using System.Windows.Forms;

namespace MiniProject
{
    public partial class Background : Form
    {
        // 유일한 인스턴스를 저장할 정적 필드
        // 인스턴스에 접근할 수 있는 속성
        private static Background instance;
        public static Background Instance
        {
            get
            {
                // 인스턴스가 없으면 생성하고, 이미 있다면 기존 인스턴스 반환
                if (instance == null)
                {
                    instance = new Background();
                }
                return instance;
            }
        }

        //LibVLC MediaPlayer 생성용
        MediaPlayer mediaPlayer;
        LibVLC libVLC;
        Media media;

        private Background()
        {
            InitializeComponent();
            InitializeMedia();

            videoView.MediaPlayer.PositionChanged += MediaPlayer_PositionChanged;
            FormClosing += Background_FormClosing;
        }

        private void MediaPlayer_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            //동영상 반복재생
            if (videoView.MediaPlayer.Position > 0.8f)
            {
                videoView.MediaPlayer.Position = 0;
            }
        }

        //영상 등록
        private void InitializeMedia()
        {
            libVLC = new LibVLC();
            mediaPlayer = new MediaPlayer(libVLC);
            string videoPath = @"C:\Users\master\Desktop\MiniProj\resource\cloud.mp4";

            media = new Media(libVLC, videoPath);

            videoView.MediaPlayer = mediaPlayer;
        }

        //video Play
        public void Video_Play()
        {
            videoView.MediaPlayer.Play(media);
        }
        //video Play
        public void Video_Stop()
        {
            videoView.MediaPlayer.Stop();
        }
        //video Pause
        public void Video_Pause()
        {
            videoView.MediaPlayer.Pause();
        }
        //video Rate
        public void Video_Rate(float backgroundPlayRate)
        {
            videoView.MediaPlayer.SetRate(backgroundPlayRate);
        }

        // 폼 종료시 LibVLC 종료
        private void Background_FormClosing(object sender, FormClosingEventArgs e)
        {
            videoView.MediaPlayer.Stop();
            libVLC.Dispose();
        }

    }
}

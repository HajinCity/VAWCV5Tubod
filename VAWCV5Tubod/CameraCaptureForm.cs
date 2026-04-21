using System;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace VAWCV5Tubod
{
    public sealed class CameraCaptureForm : Form
    {
        private readonly object frameLock = new();
        private readonly PictureBox previewPictureBox;
        private readonly ComboBox cameraComboBox;
        private readonly Label statusLabel;
        private readonly Button captureButton;
        private readonly Button cancelButton;

        private FilterInfoCollection? videoDevices;
        private VideoCaptureDevice? videoSource;
        private Bitmap? latestFrame;

        public CameraCaptureForm()
        {
            Text = "Capture Photo";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(720, 570);

            cameraComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(20, 20),
                Size = new Size(420, 25)
            };
            cameraComboBox.SelectedIndexChanged += cameraComboBox_SelectedIndexChanged;

            statusLabel = new Label
            {
                AutoSize = false,
                Location = new Point(455, 20),
                Size = new Size(245, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            previewPictureBox = new PictureBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(20, 58),
                Size = new Size(680, 430),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black
            };

            captureButton = new Button
            {
                Text = "Capture",
                Enabled = false,
                Location = new Point(515, 510),
                Size = new Size(90, 34)
            };
            captureButton.Click += captureButton_Click;

            cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(615, 510),
                Size = new Size(85, 34)
            };

            Controls.AddRange(new Control[]
            {
                cameraComboBox,
                statusLabel,
                previewPictureBox,
                captureButton,
                cancelButton
            });

            CancelButton = cancelButton;
            Load += CameraCaptureForm_Load;
            FormClosing += CameraCaptureForm_FormClosing;
        }

        public Image? CapturedImage { get; private set; }

        private void CameraCaptureForm_Load(object? sender, EventArgs e)
        {
            LoadCameraDevices();
        }

        private void LoadCameraDevices()
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                cameraComboBox.Items.Clear();

                foreach (FilterInfo device in videoDevices)
                {
                    cameraComboBox.Items.Add(device.Name);
                }

                if (cameraComboBox.Items.Count == 0)
                {
                    statusLabel.Text = "No camera found.";
                    captureButton.Enabled = false;
                    return;
                }

                statusLabel.Text = "Starting camera...";
                cameraComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Camera unavailable.";
                captureButton.Enabled = false;
                MessageBox.Show(
                    $"Unable to access the camera.{Environment.NewLine}{ex.Message}",
                    "Camera Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void cameraComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            StartSelectedCamera();
        }

        private void StartSelectedCamera()
        {
            if (videoDevices is null ||
                cameraComboBox.SelectedIndex < 0 ||
                cameraComboBox.SelectedIndex >= videoDevices.Count)
            {
                return;
            }

            StopCamera();

            videoSource = new VideoCaptureDevice(videoDevices[cameraComboBox.SelectedIndex].MonikerString);
            videoSource.NewFrame += videoSource_NewFrame;
            videoSource.Start();

            statusLabel.Text = "Camera ready.";
            captureButton.Enabled = true;
        }

        private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frameForPreview = (Bitmap)eventArgs.Frame.Clone();
            Bitmap frameForCapture = (Bitmap)eventArgs.Frame.Clone();

            lock (frameLock)
            {
                latestFrame?.Dispose();
                latestFrame = frameForCapture;
            }

            try
            {
                if (!previewPictureBox.IsDisposed && previewPictureBox.IsHandleCreated)
                {
                    previewPictureBox.BeginInvoke(new Action(() =>
                    {
                        if (previewPictureBox.IsDisposed)
                        {
                            frameForPreview.Dispose();
                            return;
                        }

                        Image? previousImage = previewPictureBox.Image;
                        previewPictureBox.Image = frameForPreview;
                        previousImage?.Dispose();
                    }));
                }
                else
                {
                    frameForPreview.Dispose();
                }
            }
            catch (ObjectDisposedException)
            {
                frameForPreview.Dispose();
            }
            catch (InvalidOperationException)
            {
                frameForPreview.Dispose();
            }
        }

        private void captureButton_Click(object? sender, EventArgs e)
        {
            Bitmap? capturedFrame;

            lock (frameLock)
            {
                capturedFrame = latestFrame is null ? null : new Bitmap(latestFrame);
            }

            if (capturedFrame is null)
            {
                MessageBox.Show(
                    "The camera has not produced a frame yet. Please try again.",
                    "Capture Not Ready",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            CapturedImage?.Dispose();
            CapturedImage = capturedFrame;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CameraCaptureForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopCamera();
            ClearPreviewImage();

            lock (frameLock)
            {
                latestFrame?.Dispose();
                latestFrame = null;
            }
        }

        private void StopCamera()
        {
            if (videoSource is null)
            {
                return;
            }

            videoSource.NewFrame -= videoSource_NewFrame;

            if (videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }

            videoSource = null;
            captureButton.Enabled = false;
        }

        private void ClearPreviewImage()
        {
            Image? previewImage = previewPictureBox.Image;
            previewPictureBox.Image = null;
            previewImage?.Dispose();
        }
    }
}

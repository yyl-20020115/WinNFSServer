namespace WinNFSServer;
using LibWinNFSServer;
public partial class MainForm : Form
{
    private readonly NFSServer server = new();

    public MainForm()
    {
        InitializeComponent();
    }

    private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
    {

    }

    private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
    {

    }

    private void MainForm_Load(object sender, EventArgs e)
    {

    }


    private void UpdateData()
    {
        this.server.SocketAddress = this.textBoxIP.Text;
        this.server.UID = uint.TryParse(this.textBoxUID.Text, out var uid) ? uid : 0;
        this.server.GID = uint.TryParse(this.textBoxUID.Text, out var gid) ? gid : 0;
    }

    private void ButtonStart_Click(object sender, EventArgs e)
    {

        this.UpdateData();
        this.server.Start([(".\\", "")]);
        this.buttonStart.Enabled = false;
        this.buttonStop.Enabled = true;
    }

    private void ButtonStop_Click(object sender, EventArgs e)
    {
        this.server.Stop();
        this.buttonStart.Enabled = true;
        this.buttonStop.Enabled = false;
    }

    private void ButtonConfirm_Click(object sender, EventArgs e)
    {
        this.UpdateData();
        this.Hide();
    }

    private void ButtonCancel_Click(object sender, EventArgs e)
    {
        this.Hide();
    }
}

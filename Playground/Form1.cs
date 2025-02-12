namespace Playground
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            try
            {
                //new GoogleTextToSpeech.TextToSpeechService().ConvertTextToSpeech("Καλημέρα.Σήμερα είναι Δευτέρα!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

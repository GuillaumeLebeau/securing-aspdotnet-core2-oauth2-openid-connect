namespace ImageGallery.Client.Models
{
    public class OrderFrameViewModel
    {
        public OrderFrameViewModel(string address)
        {
            Address = address;
        }

        public string Address { get; private set; } = string.Empty;
    }
}
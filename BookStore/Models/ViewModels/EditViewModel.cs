using BookStore.Models.Domain;

namespace BookStore.Models.ViewModels
{
    public class EditViewModel : Book
    {
        public string ExistingImagePath { get; set; }
    }
}

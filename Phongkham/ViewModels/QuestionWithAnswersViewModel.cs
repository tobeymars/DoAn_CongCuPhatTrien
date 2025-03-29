namespace Phongkham.ViewModels
{
    public class QuestionWithAnswersViewModel
    {
        public int Id { get; set; }
        public string QuestionContent { get; set; }
        public string PatientName { get; set; }
        public DateTime DateAsked { get; set; }
        public bool IsAnswered { get; set; }
        public List<AnswerViewModel> Answers { get; set; }
    }
    public class AnswerViewModel
    {
        public string Content { get; set; }
        public string DentistName { get; set; }
        public DateTime DateAnswered { get; set; }
    }
}

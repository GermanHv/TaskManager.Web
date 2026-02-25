namespace TaskManager.Web.Models
{
    public class TaskFormViewModel
    {
        // 0 = modo crear, >0 = modo editar
        public int Id { get; set; } = 0;

        public string Title { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public int Step { get; set; }

        public bool IsCompleted { get; set; } = false;
    }
}
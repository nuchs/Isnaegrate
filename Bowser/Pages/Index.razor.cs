namespace Bowser.Pages
{
    public partial class Index
    {
        public IEnumerable<(string Id, string Label)> Users
            => repo.Select(u => (u.Id, $"{u.Org} | {u.Title} {u.Name}")).OrderBy(u => u.Item2);

        public string SelectedId { get; set; } = "";

        public void Logon()
        {
            nav.NavigateTo($"/content/{SelectedId}");
        }
    }
}
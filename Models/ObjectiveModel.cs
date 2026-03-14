using System.Linq;

namespace AngelPearl.Models
{
    public class ObjectiveModel
    {
        public ObjectiveModel(string id, string headline)
        {
            Id = id;
			Headline.Value = headline;
        }

        public string Id { get; set; }

        public ModelProperty<string> Headline { get; set; } = new ModelProperty<string>("- Blank Objective");


		public ModelProperty<bool> Completed { get; set; } = new ModelProperty<bool>(false);
	}
}


namespace recipeservice.Model
{
    public class Parameter
    {
        public int parameterId { get; set; }
        public string parameterName { get; set; }
        public string ParameterCode { get; set; }
        public int thingGroupId { get; set; }
        public ThingGroup thingGroup { get; set; }

    }
}
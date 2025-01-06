using System;
namespace LukeDeaneTrivia
{
	public class QuestionResponse
	{
        public int ResponseCode { get; set; }
        public List<Question> results { get; set; }
    }
}


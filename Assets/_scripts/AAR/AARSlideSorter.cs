using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class AARSlideSorter 
{

	public static List<AARSlide> FilterContainer(SlideContainer container, int aar)
	{
		List<AARSlide> allSlides = new List<AARSlide>();

		if(container.basicSlides != null)
			foreach(AARSlide slide in container.basicSlides)
				allSlides.Add(slide);

		if(container.movieSlides.Length > 0)
			foreach(AARSlide slide in container.movieSlides)
				allSlides.Add(slide);

		if(container.multipleChoiceQuizzes.Length > 0)
			foreach(AARSlide slide in container.multipleChoiceQuizzes)
				allSlides.Add(slide);

		if(container.sliderQuizzes != null)
			foreach(AARSlide slide in container.sliderQuizzes)
				allSlides.Add(slide);

		if(container.pathfinders != null)
			foreach(AARSlide slide in container.pathfinders)
				allSlides.Add(slide);

		if(container.specialSlides != null)
			foreach(AARSlide slide in container.specialSlides)
				allSlides.Add(slide);

		Debug.Log ("All Slides: " + allSlides.Count);

		List<AARSlide> filteredSlides = new List<AARSlide>();
		
		foreach(AARSlide slide in allSlides) {
			if(slide.AAR == aar) {
				filteredSlides.Add(slide);
			}
		}

		Debug.Log ("Filtered Slides: " + filteredSlides.Count);

		return filteredSlides;
	}

	public static int FindStartIndex(List<AARSlide> slides)
	{
		int lowIndex = int.MaxValue;
		
		foreach(AARSlide slide in slides){
			if(slide.slideIndex < lowIndex)
			{
				lowIndex = slide.slideIndex;
			}
		}
		
		return lowIndex;
	}

	public static AARSlide FindSlide(List<AARSlide> slides, int index)
	{
		foreach(AARSlide slide in slides) {
			if(slide.slideIndex == index)
				return slide;
		}



		Debug.LogWarning("Unable to find slide " + index.ToString() + "!");
		return null;
	}

}

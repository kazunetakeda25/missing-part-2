using UnityEngine;
using System.Collections;

namespace AC
{

	public interface ISave
	{

		void PreSave ();
		void PostLoad ();

	}


	public interface ISaveOptions
	{
		
		void PreSaveOptions ();
		void PostLoadOptions ();
		
	}

}
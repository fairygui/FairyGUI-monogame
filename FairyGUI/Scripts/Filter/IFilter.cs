namespace FairyGUI
{
	public interface IFilter
	{
		/// <summary>
		/// 
		/// </summary>
		DisplayObject target { get; set; }

		/// <summary>
		/// 
		/// </summary>
		void Update();

		/// <summary>
		/// 
		/// </summary>
		void Apply(FairyBatch batch);

		/// <summary>
		/// 
		/// </summary>
		void Dispose();
	}
}

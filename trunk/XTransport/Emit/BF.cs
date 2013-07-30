using System.Reflection;

namespace XTransport.Emit
{
	public class BF
	{
		public const BindingFlags DeclOnly = BindingFlags.DeclaredOnly;

		public const BindingFlags All = Public | Private;
		public const BindingFlags AllInstance = PublicInstance | PrivateInstance;
		public const BindingFlags AllStatic = PublicStatic | PrivateStatic;

		public const BindingFlags Public = PublicInstance | PublicStatic;
		public const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
		public const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

		public const BindingFlags Protected = ProtectedInstance | ProtectedStatic;
		public const BindingFlags ProtectedInstance = BindingFlags.NonPublic | BindingFlags.Instance;
		public const BindingFlags ProtectedStatic = BindingFlags.NonPublic | BindingFlags.Static;

		public const BindingFlags Private = PrivateInstance | PrivateStatic;
		public const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
		public const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
	}
}
using System.Collections.Generic;

public enum UnitType
{
	//!!!!!!순서 막 바꾸지 말것!!!!!! Enum은 serialize하면 순서 바뀌면 다 밀림.
	//뒤에 추가만 하기
	None = 0,
	SpearMan = 1, //버티컬
	ArcherMan,    //버티컬
	KingMan,
	HorseMan,     // 버티컬
	SwordMan,     // 버티컬
	CavalierMan,
	CrossBowMan,
	HalberdMan,
	ShieldMan,
	PrinceMan,
	AssassinMan,
	SniperMan,
	PriestMan,
	MagicMan,
	ButcherZombie,
	Skeleton,       // 버티컬 안씀
	SkeletonArcher, // 버티컬 안씀
	Lich,
	DreadKnight,
	DeathKnight,
	Assassin,       // 버티컬
	TwoSwordAssassin, 
	PriestSuccubus, 
	FlightSword,
	Golem, 
	FlameMagician, 
}

public class UnitTypeComparer : IComparer<UnitType>
{
	public static readonly UnitTypeComparer Default = new();
	
	public int Compare(UnitType x, UnitType y)
	{
		int left = GetOrder(x);
		int right = GetOrder(y);
		
		if (left < right)
		{
			return -1;
		}
		else if (left > right)
		{
			return 1;
		}
		else
		{
			return 0;
		}
	}
	
	private static int GetOrder(UnitType type)
	{
		return type switch
		{
			UnitType.SwordMan => 0,
			UnitType.Assassin => 1,
			UnitType.ArcherMan => 2,
			
			_ => (int)type
		};
	}
}
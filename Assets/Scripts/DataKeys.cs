using AutoUI.Data;
using CellularAutomata;

namespace DefaultNamespace {
[DataKeyRegistry]
public class DataKeys {
	public static DataKey<BaseBuilderAutomata> AUTOMATA = new DataKey<BaseBuilderAutomata>("automata");
}
}
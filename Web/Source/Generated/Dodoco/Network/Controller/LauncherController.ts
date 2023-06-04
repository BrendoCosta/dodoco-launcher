//     This code was generated by a Reinforced.Typings tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { Nullable } from '@Dodoco/index';
import { RpcClient } from '@Dodoco/Backend';
import { ILauncher } from '../../Launcher/ILauncher';
import { LauncherActivityState } from '../../Launcher/LauncherActivityState';
import { LauncherCache } from '../../Launcher/Cache/LauncherCache';
import { LauncherExecutionState } from '../../Launcher/LauncherExecutionState';
import { IGame } from '../../Game/IGame';
import { LauncherSettings } from '../../Launcher/Settings/LauncherSettings';

export class LauncherController
{
	public async GetEntityInstance() : Promise<ILauncher>
	{
		return await RpcClient.GetInstance().CallAsync<ILauncher>("Dodoco.Network.Controller.LauncherController.GetEntityInstance", []);
	}
	public async IsRunning() : Promise<boolean>
	{
		return await RpcClient.GetInstance().CallAsync<boolean>("Dodoco.Network.Controller.LauncherController.IsRunning", []);
	}
	public async GetLauncherActivityState() : Promise<LauncherActivityState>
	{
		return await RpcClient.GetInstance().CallAsync<LauncherActivityState>("Dodoco.Network.Controller.LauncherController.GetLauncherActivityState", []);
	}
	public async GetLauncherCache() : Promise<LauncherCache>
	{
		return await RpcClient.GetInstance().CallAsync<LauncherCache>("Dodoco.Network.Controller.LauncherController.GetLauncherCache", []);
	}
	public async GetContent() : Promise<any>
	{
		return await RpcClient.GetInstance().CallAsync<any>("Dodoco.Network.Controller.LauncherController.GetContent", []);
	}
	public async GetLauncherExecutionState() : Promise<LauncherExecutionState>
	{
		return await RpcClient.GetInstance().CallAsync<LauncherExecutionState>("Dodoco.Network.Controller.LauncherController.GetLauncherExecutionState", []);
	}
	public async GetGame() : Promise<IGame>
	{
		return await RpcClient.GetInstance().CallAsync<IGame>("Dodoco.Network.Controller.LauncherController.GetGame", []);
	}
	public async GetResource() : Promise<any>
	{
		return await RpcClient.GetInstance().CallAsync<any>("Dodoco.Network.Controller.LauncherController.GetResource", []);
	}
	public async GetLauncherSettings() : Promise<LauncherSettings>
	{
		return await RpcClient.GetInstance().CallAsync<LauncherSettings>("Dodoco.Network.Controller.LauncherController.GetLauncherSettings", []);
	}
	public async RepairGameFiles() : Promise<void>
	{
		return await RpcClient.GetInstance().CallAsync<void>("Dodoco.Network.Controller.LauncherController.RepairGameFiles", []);
	}
	constructor () { } 
	private static instance: Nullable<LauncherController> = null;
	public static GetInstance() : LauncherController
	{
		if (LauncherController.instance == null) LauncherController.instance = new LauncherController(); return LauncherController.instance;
	}
}

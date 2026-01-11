using System.Runtime.InteropServices;

namespace NAutoSuite.Hardware.Leadshine;

/// <summary>
/// Wrapper class for LTDMC.dll - Leadshine Motion Control Library
/// Includes P/Invoke declarations for EtherCAT Motion Controller
/// </summary>
public static partial class LTDMC
{
    private const string DllName = "LTDMC.dll";

    /// <summary>
    /// Delegate for interrupt callback function
    /// </summary>
    public delegate void DMC3K5K_OPERATE(IntPtr operate_data);

    #region Board Initialization and Configuration

    [DllImport(DllName, EntryPoint = "dmc_set_debug_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_debug_mode(ushort mode, string FileName);

    [DllImport(DllName, EntryPoint = "dmc_get_debug_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_debug_mode(ref ushort mode, IntPtr FileName);

    [DllImport(DllName, EntryPoint = "dmc_board_init", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_board_init();

    [DllImport(DllName, EntryPoint = "dmc_board_init_eth", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_board_init_eth(ushort CardNo, string ipaddr);

    [DllImport(DllName, EntryPoint = "dmc_board_reset", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_board_reset();

    [DllImport(DllName, EntryPoint = "dmc_board_close", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_board_close();

    [DllImport(DllName)]
    public static extern short dmc_soft_reset(ushort CardNo);

    [DllImport(DllName)]
    public static extern short dmc_cool_reset(ushort CardNo);

    [DllImport(DllName, EntryPoint = "dmc_original_reset", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_original_reset(ushort CardNo);

    [DllImport(DllName, EntryPoint = "dmc_get_CardInfList", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_CardInfList(ref ushort CardNum, uint[] CardTypeList, ushort[] CardIdList);

    [DllImport(DllName, EntryPoint = "dmc_get_card_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_card_version(ushort CardNo, ref uint CardVersion);

    [DllImport(DllName, EntryPoint = "dmc_get_card_soft_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_card_soft_version(ushort CardNo, ref uint FirmID, ref uint SubFirmID);

    [DllImport(DllName, EntryPoint = "dmc_get_card_lib_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_card_lib_version(ref uint LibVer);

    [DllImport(DllName, EntryPoint = "dmc_get_total_axes", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_total_axes(ushort CardNo, ref uint TotalAxis);

    [DllImport(DllName, EntryPoint = "dmc_get_total_ionum", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_total_ionum(ushort CardNo, ref ushort TotalIn, ref ushort TotalOut);

    #endregion

    #region Pulse Mode Configuration

    [DllImport(DllName, EntryPoint = "dmc_set_pulse_outmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_pulse_outmode(ushort CardNo, ushort axis, ushort outmode);

    [DllImport(DllName, EntryPoint = "dmc_get_pulse_outmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_pulse_outmode(ushort CardNo, ushort axis, ref ushort outmode);

    [DllImport(DllName, EntryPoint = "dmc_get_equiv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_equiv(ushort CardNo, ushort axis, ref double equiv);

    [DllImport(DllName, EntryPoint = "dmc_set_equiv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_equiv(ushort CardNo, ushort axis, double equiv);

    #endregion

    #region Soft Limit and Emergency

    [DllImport(DllName, EntryPoint = "dmc_set_softlimit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_softlimit(ushort CardNo, ushort axis, ushort enable, ushort source_sel, ushort SL_action, int N_limit, int P_limit);

    [DllImport(DllName, EntryPoint = "dmc_get_softlimit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_softlimit(ushort CardNo, ushort axis, ref ushort enable, ref ushort source_sel, ref ushort SL_action, ref int N_limit, ref int P_limit);

    [DllImport(DllName)]
    public static extern short dmc_set_softlimit_unit(ushort CardNo, ushort axis, ushort enable, ushort source_sel, ushort SL_action, double N_limit, double P_limit);

    [DllImport(DllName)]
    public static extern short dmc_get_softlimit_unit(ushort CardNo, ushort axis, ref ushort enable, ref ushort source_sel, ref ushort SL_action, ref double N_limit, ref double P_limit);

    [DllImport(DllName, EntryPoint = "dmc_set_el_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_el_mode(ushort CardNo, ushort axis, ushort el_enable, ushort el_logic, ushort el_mode);

    [DllImport(DllName, EntryPoint = "dmc_get_el_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_el_mode(ushort CardNo, ushort axis, ref ushort el_enable, ref ushort el_logic, ref ushort el_mode);

    [DllImport(DllName, EntryPoint = "dmc_set_emg_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_emg_mode(ushort CardNo, ushort axis, ushort enable, ushort emg_logic);

    [DllImport(DllName, EntryPoint = "dmc_get_emg_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_emg_mode(ushort CardNo, ushort axis, ref ushort enable, ref ushort emg_logic);

    #endregion

    #region Single Axis Motion

    [DllImport(DllName, EntryPoint = "dmc_set_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_profile(ushort CardNo, ushort axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double stop_vel);

    [DllImport(DllName, EntryPoint = "dmc_get_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_profile(ushort CardNo, ushort axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double stop_vel);

    [DllImport(DllName, EntryPoint = "dmc_set_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_profile_unit(ushort CardNo, ushort Axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);

    [DllImport(DllName, EntryPoint = "dmc_get_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_profile_unit(ushort CardNo, ushort Axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);

    [DllImport(DllName, EntryPoint = "dmc_set_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_s_profile(ushort CardNo, ushort axis, ushort s_mode, double s_para);

    [DllImport(DllName, EntryPoint = "dmc_get_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_s_profile(ushort CardNo, ushort axis, ushort s_mode, ref double s_para);

    [DllImport(DllName, EntryPoint = "dmc_pmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_pmove(ushort CardNo, ushort axis, int Dist, ushort posi_mode);

    [DllImport(DllName, EntryPoint = "dmc_pmove_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_pmove_unit(ushort CardNo, ushort axis, double Dist, ushort posi_mode);

    [DllImport(DllName, EntryPoint = "dmc_change_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_change_speed(ushort CardNo, ushort axis, double Curr_Vel, double Taccdec);

    [DllImport(DllName, EntryPoint = "dmc_change_speed_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_change_speed_unit(ushort CardNo, ushort Axis, double New_Vel, double Taccdec);

    #endregion

    #region JOG Motion

    [DllImport(DllName, EntryPoint = "dmc_vmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_vmove(ushort CardNo, ushort axis, ushort dir);

    #endregion

    #region Homing

    [DllImport(DllName, EntryPoint = "dmc_set_home_pin_logic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_home_pin_logic(ushort CardNo, ushort axis, ushort org_logic, double filter);

    [DllImport(DllName, EntryPoint = "dmc_get_home_pin_logic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_home_pin_logic(ushort CardNo, ushort axis, ref ushort org_logic, ref double filter);

    [DllImport(DllName, EntryPoint = "dmc_set_homemode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_homemode(ushort CardNo, ushort axis, ushort home_dir, double vel, ushort mode, ushort EZ_count);

    [DllImport(DllName, EntryPoint = "dmc_get_homemode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_homemode(ushort CardNo, ushort axis, ref ushort home_dir, ref double vel, ref ushort home_mode, ref ushort EZ_count);

    [DllImport(DllName, EntryPoint = "dmc_home_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_home_move(ushort CardNo, ushort axis);

    [DllImport(DllName)]
    public static extern short dmc_get_home_result(ushort CardNo, ushort axis, ref ushort state);

    #endregion

    #region Status Reading

    [DllImport(DllName, EntryPoint = "dmc_get_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int dmc_get_position(ushort CardNo, ushort axis);

    [DllImport(DllName)]
    public static extern short dmc_get_position_unit(ushort CardNo, ushort axis, ref double pos);

    [DllImport(DllName, EntryPoint = "dmc_check_done", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_check_done(ushort CardNo, ushort axis);

    [DllImport(DllName)]
    public static extern uint dmc_axis_io_status(ushort CardNo, ushort axis);

    [DllImport(DllName)]
    public static extern uint dmc_get_axis_run_mode(ushort CardNo, ushort axis);

    #endregion

    #region Stop Commands

    [DllImport(DllName, EntryPoint = "dmc_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_stop(ushort CardNo, ushort axis, ushort stop_mode);

    [DllImport(DllName)]
    public static extern short dmc_emg_stop(ushort CardNo);

    #endregion

    #region Servo Control

    [DllImport(DllName)]
    public static extern short dmc_set_sevon_enable(ushort CardNo, ushort axis, ushort on_off);

    #endregion

    #region IO Control

    [DllImport(DllName)]
    public static extern uint dmc_read_inbit(ushort CardNo, ushort bitno);

    [DllImport(DllName)]
    public static extern short dmc_write_outbit(ushort CardNo, ushort bitno, ushort on_off);

    [DllImport(DllName)]
    public static extern uint dmc_read_outbit(ushort CardNo, ushort bitno);

    [DllImport(DllName)]
    public static extern uint dmc_read_inport(ushort CardNo, ushort portno);

    [DllImport(DllName)]
    public static extern short dmc_write_outport(ushort CardNo, ushort portno, uint outport_val);

    [DllImport(DllName)]
    public static extern uint dmc_read_outport(ushort CardNo, ushort portno);

    #endregion
}

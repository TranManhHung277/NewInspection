using System.Runtime.InteropServices;

namespace NAutoSuite.Hardware.Leadshine;

public static partial class LTDMC
{
    /// <summary>
    /// Delegate cho interrupt callback function
    /// </summary>
    public delegate void DMC3K5K_OPERATE(IntPtr operate_data);

    // Thiết lập và đọc chế độ in Debug (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_debug_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_debug_mode(UInt16 mode, string FileName);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_debug_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_debug_mode(ref UInt16 mode, IntPtr FileName);

    //--------------------- Hàm khởi tạo và cấu hình Board mạch ----------------------
    // Khởi tạo Board điều khiển (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_board_init", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_board_init();
    [DllImport("LTDMC.dll", EntryPoint = "dmc_board_init_eth", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_board_init_eth(ushort CardNo, string ipaddr);
    // Reset phần cứng (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_board_reset", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_board_reset();
    // Đóng Board điều khiển (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_board_close", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_board_close();
    // Reset mềm Board điều khiển (Áp dụng cho thẻ Bus EtherCAT, RTEX)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_soft_reset(ushort CardNo);
    // Reset lạnh Board điều khiển (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_cool_reset(ushort CardNo);
    // Reset gốc Board điều khiển (Áp dụng cho thẻ Bus EtherCAT)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_original_reset", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_original_reset(ushort CardNo);
    // Đọc danh sách thông tin Board (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_CardInfList", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_CardInfList(ref UInt16 CardNum, UInt32[] CardTypeList, UInt16[] CardIdList);
    // Đọc phiên bản phát hành (Áp dụng cho dòng DMC3000/DMC5X10 Pulse, thẻ Bus EtherCAT)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_card_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_card_version(UInt16 CardNo, ref UInt32 CardVersion);
    // Đọc phiên bản Firmware của phần cứng Board (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_card_soft_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_card_soft_version(UInt16 CardNo, ref UInt32 FirmID, ref UInt32 SubFirmID);
    // Đọc phiên bản thư viện DLL của Board (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_card_lib_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_card_lib_version(ref UInt32 LibVer);
    // Đọc phiên bản phát hành (Áp dụng cho dòng DMC3000/DMC5X10 Pulse, thẻ Bus EtherCAT)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_release_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_release_version(ushort ConnectNo, byte[] ReleaseVersion);
    // Đọc tổng số trục của Board (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_total_axes", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_total_axes(UInt16 CardNo, ref UInt32 TotalAxis);
    // Lấy số lượng IO cục bộ (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_total_ionum", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_total_ionum(ushort CardNo, ref ushort TotalIn, ref ushort TotalOut);
    // Lấy số lượng ADDA cục bộ (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_total_adcnum", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_total_adcnum(ushort CardNo, ref ushort TotalIn, ref ushort TotalOut);
    // Đọc hệ số tọa độ nội suy của Board (Dành riêng)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_total_liners", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_total_liners(UInt16 CardNo, ref UInt32 TotalLiner);
    // Các hàm tùy chỉnh (Dành riêng)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_board_init_onecard", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_board_init_onecard(ushort CardNo);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_board_close_onecard", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_board_close_onecard(ushort CardNo);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_board_reset_onecard", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_board_reset_onecard(ushort CardNo);

    // Các hàm mật khẩu (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_write_sn", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_write_sn(UInt16 CardNo, string new_sn);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_check_sn", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_check_sn(UInt16 CardNo, string check_sn);
    // Nhập mật khẩu sn20191101 (Áp dụng cho dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_enter_password_ex(UInt16 CardNo, string str_pass);

    //--------------------- Mô-đun chuyển động: Chế độ Pulse (Xung) ------------------
    // Chế độ đầu ra xung (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_pulse_outmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_pulse_outmode(UInt16 CardNo, UInt16 axis, UInt16 outmode);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_pulse_outmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_pulse_outmode(UInt16 CardNo, UInt16 axis, ref UInt16 outmode);
    // Đơn vị tương đương xung (Pulse Equivalent - Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_equiv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_equiv(UInt16 CardNo, UInt16 axis, ref double equiv);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_equiv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_equiv(UInt16 CardNo, UInt16 axis, double equiv);
    // Bù khe hở Backlash (Đơn vị xung - Áp dụng dòng DMC5000 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_backlash_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_backlash_unit(UInt16 CardNo, UInt16 axis, double backlash);
    [DllImport("LeTDMC.dll", EntryPoint = "dmc_get_backlash_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_backlash_unit(UInt16 CardNo, UInt16 axis, ref double backlash);

    // Tải xuống tệp thông dụng
    [DllImport("LTDMC.dll", EntryPoint = "dmc_download_file", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_download_file(ushort CardNo, string pfilename, byte[] pfilenameinControl, ushort filetype);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_upload_file", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_upload_file(ushort CardNo, string pfilename, byte[] pfilenameinControl, ushort filetype);
    // Tải xuống tệp bộ nhớ thẻ Bus (Áp dụng cho thẻ EtherCAT)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_download_memfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_download_memfile(ushort CardNo, byte[] pbuffer, uint buffsize, byte[] pfilenameinControl, ushort filetype);
    // Tải lên tệp bộ nhớ (Áp dụng cho thẻ EtherCAT)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_upload_memfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_upload_memfile(ushort CardNo, byte[] pbuffer, uint buffsize, byte[] pfilenameinControl, ref uint puifilesize, ushort filetype);
    // Lấy tiến độ tệp (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_progress(ushort CardNo, ref float process);
    // Tải xuống tệp cấu hình (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_download_configfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_download_configfile(UInt16 CardNo, String FileName);
    // Tải xuống tệp Firmware (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_download_firmware", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_download_firmware(UInt16 CardNo, String FileName);

    //---------------------- Thiết lập giới hạn mềm (Soft Limit) và Ngoại lệ -------------------------------
    // Thiết lập/Đọc tham số giới hạn mềm (Áp dụng thẻ E3032, R3032, dòng DMC3000/5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_softlimit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_softlimit(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 source_sel, UInt16 SL_action, Int32 N_limit, Int32 P_limit);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_softlimit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_softlimit(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 source_sel, ref UInt16 SL_action, ref Int32 N_limit, ref Int32 P_limit);
    // Thiết lập/Đọc tham số giới hạn mềm theo Unit (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_softlimit_unit(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 source_sel, UInt16 SL_action, double N_limit, double P_limit);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_softlimit_unit(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 source_sel, ref UInt16 SL_action, ref double N_limit, ref double P_limit);
    // Thiết lập/Đọc tín hiệu EL (External Limit - Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_el_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_el_mode(UInt16 CardNo, UInt16 axis, UInt16 el_enable, UInt16 el_logic, UInt16 el_mode);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_el_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_el_mode(UInt16 CardNo, UInt16 axis, ref UInt16 el_enable, ref UInt16 el_logic, ref UInt16 el_mode);
    // Thiết lập/Đọc tín hiệu EMG (Emergency Stop - Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_emg_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_emg_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 emg_logic);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_emg_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_emg_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enbale, ref UInt16 emg_logic);
    // Thiết lập tín hiệu dừng giảm tốc bên ngoài (DSTP) và thời gian (ms) (Dành riêng)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_dstp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_dstp_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 logic, UInt32 time);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_dstp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_dstp_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 logic, ref UInt32 time);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_dstp_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_dstp_time(UInt16 CardNo, UInt16 axis, UInt32 time);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_dstp_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_dstp_time(UInt16 CardNo, UInt16 axis, ref UInt32 time);
    // Thiết lập tín hiệu dừng giảm tốc IO bên ngoài và thời gian (s) (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_io_dstp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_io_dstp_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 logic);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_io_dstp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_io_dstp_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 logic);
    // Thiết lập/Đọc thời gian giảm tốc dừng Pmove (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_dec_stop_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_dec_stop_time(UInt16 CardNo, UInt16 axis, double stop_time);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_dec_stop_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_dec_stop_time(UInt16 CardNo, UInt16 axis, ref double stop_time);
    // Thiết lập/Đọc thời gian giảm tốc dừng vector nội suy (Áp dụng dòng DMC5X10 Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_vector_dec_stop_time(UInt16 CardNo, UInt16 Crd, double stop_time);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_vector_dec_stop_time(UInt16 CardNo, UInt16 Crd, ref double stop_time);
    // Khoảng cách dừng giảm tốc IO (Áp dụng dòng DMC3000, DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_dec_stop_dist(UInt16 CardNo, UInt16 axis, Int32 dist);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_dec_stop_dist(UInt16 CardNo, UInt16 axis, ref Int32 dist);
    // Dừng IO chính xác, hỗ trợ chuyển động pmvove/vmove (Áp dụng dòng DMC3000, DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_io_exactstop(UInt16 CardNo, UInt16 axis, UInt16 ioNum, UInt16[] ioList, UInt16 enable, UInt16 valid_logic, UInt16 action, UInt16 move_dir);
    // Thiết lập cổng IO dừng giảm tốc tại bit cụ thể (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_io_dstp_bitno(UInt16 CardNo, UInt16 axis, UInt16 bitno, double filter);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_io_dstp_bitno(UInt16 CardNo, UInt16 axis, ref UInt16 bitno, ref double filter);

    //--------------------------- Chuyển động đơn trục ----------------------
    // Thiết lập/Đọc tham số đường cong vận tốc (Profile) (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_profile(UInt16 CardNo, UInt16 axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double stop_vel);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_profile(UInt16 CardNo, UInt16 axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double stop_vel);
    // Thiết lập vận tốc (Đơn vị Unit) (Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_profile_unit(UInt16 CardNo, UInt16 Axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_profile_unit(UInt16 CardNo, UInt16 Axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);
    // Thiết lập đường cong vận tốc, biểu thị giá trị gia tốc (Xung) (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_acc_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_acc_profile(UInt16 CardNo, UInt16 axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double stop_vel);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_acc_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_acc_profile(UInt16 CardNo, UInt16 axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double stop_vel);
    // Thiết lập đường cong vận tốc, biểu thị giá trị gia tốc (Unit) (Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_profile_unit_acc", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_profile_unit_acc(UInt16 CardNo, UInt16 Axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_profile_unit_acc", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_profile_unit_acc(UInt16 CardNo, UInt16 Axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);
    // Thiết lập/Đọc tham số đường cong vận tốc làm mượt S-curve (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_s_profile(UInt16 CardNo, UInt16 axis, UInt16 s_mode, double s_para);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_s_profile(UInt16 CardNo, UInt16 axis, UInt16 s_mode, ref double s_para);
    // Chuyển động điểm (Pmove - Xung) (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_pmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_pmove(UInt16 CardNo, UInt16 axis, Int32 Dist, UInt16 posi_mode);
    // Chuyển động điểm (Pmove - Unit) (Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_pmove_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_pmove_unit(UInt16 CardNo, UInt16 axis, double Dist, UInt16 posi_mode);
    // Chuyển động trục theo độ dài cố định, đồng thời gửi vận tốc và thời gian S (Xung) (Áp dụng dòng DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pmove_extern(UInt16 CardNo, UInt16 axis, double dist, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double stop_Vel, double s_para, UInt16 posi_mode);
    // Thay đổi vị trí đích Online (Xung), thay đổi vị trí đích khi đang chuyển động (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_reset_target_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_reset_target_position(UInt16 CardNo, UInt16 axis, Int32 dist, UInt16 posi_mode);
    // Thay đổi vị trí đích & vận tốc (Unit) (Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_reset_target_position_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_reset_target_position_unit(UInt16 CardNo, UInt16 Axis, double New_Pos);
    // Thay đổi vận tốc Online (Xung), thay đổi vận tốc hiện tại của trục chỉ định khi đang chuyển động (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_change_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_change_speed(UInt16 CardNo, UInt16 axis, double Curr_Vel, double Taccdec);
    // Thay đổi vận tốc Online (Unit), thay đổi vận tốc hiện tại của trục chỉ định khi đang chuyển động (Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_change_speed_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_change_speed_unit(UInt16 CardNo, UInt16 Axis, double New_Vel, double Taccdec);
    // Cưỡng bức thay đổi vị trí đích dù đang chuyển động hay không (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_update_target_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_update_target_position(UInt16 CardNo, UInt16 axis, Int32 dist, UInt16 posi_mode);
    // Cưỡng bức thay đổi vị trí mở rộng (Áp dụng dòng DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_update_target_position_extern(UInt16 CardNo, UInt16 axis, double mid_pos, double aim_pos, double vel, UInt16 posi_mode);
    // Thay đổi vận tốc Online (Unit), thay đổi vận tốc hiện tại của trục chỉ định khi đang chuyển động (Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_update_target_position_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_update_target_position_unit(UInt16 CardNo, UInt16 Axis, double New_Pos);

    //--------------------- Chuyển động JOG --------------------
    // Chuyển động vận tốc liên tục đơn trục (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_vmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_vmove(UInt16 CardNo, UInt16 axis, UInt16 dir);

    //--------------------- Chuyển động nội suy (Interpolation) --------------------
    // Thiết lập vận tốc nội suy (Xung) (Áp dụng dòng DMC3000 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_vector_profile_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_vector_profile_multicoor(UInt16 CardNo, UInt16 Crd, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_vector_profile_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_vector_profile_multicoor(UInt16 CardNo, UInt16 Crd, ref double Min_Vel, ref double Max_Vel, ref double Taccdec, ref double Tdec, ref double Stop_Vel);
    // Thiết lập/Đọc tham số đường cong vận tốc làm mượt S-curve (Áp dụng dòng DMC3000 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_vector_s_profile_multicoor(UInt16 CardNo, UInt16 Crd, UInt16 s_mode, double s_para);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_vector_s_profile_multicoor(UInt16 CardNo, UInt16 Crd, UInt16 s_mode, ref double s_para);
    // Tham số vận tốc nội suy (Unit) (Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_vector_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_vector_profile_unit(UInt16 CardNo, UInt16 Crd, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_vector_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_vector_profile_unit(UInt16 CardNo, UInt16 Crd, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);
    // Thiết lập tham số đường cong vận tốc làm mượt S-curve (Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_vector_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_vector_s_profile(UInt16 CardNo, UInt16 Crd, UInt16 s_mode, double s_para);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_vector_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_vector_s_profile(UInt16 CardNo, UInt16 Crd, UInt16 s_mode, ref double s_para);
    // Chuyển động nội suy đường thẳng (Áp dụng dòng DMC3000 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_line_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_line_multicoor(UInt16 CardNo, UInt16 crd, UInt16 axisNum, UInt16[] axisList, Int32[] DistList, UInt16 posi_mode);
    // Chuyển động nội suy cung tròn (Áp dụng dòng DMC3000 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_arc_move_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_arc_move_multicoor(UInt16 CardNo, UInt16 crd, UInt16[] AxisList, Int32[] Target_Pos, Int32[] Cen_Pos, UInt16 Arc_Dir, UInt16 posi_mode);
    // Nội suy đường thẳng (Unit) (Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_line_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_line_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, UInt16 posi_mode);
    // Nội suy cung tròn xác định tâm (Unit) (Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_arc_move_center_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_arc_move_center_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double[] Cen_Pos, UInt16 Arc_Dir, Int32 Circle, UInt16 posi_mode);
    // Nội suy cung tròn xác định bán kính (Unit) (Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_arc_move_radius_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_arc_move_radius_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double Arc_Radius, UInt16 Arc_Dir, Int32 Circle, UInt16 posi_mode);
    // Nội suy cung tròn qua 3 điểm (Unit) (Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_arc_move_3points_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_arc_move_3points_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double[] Mid_Pos, Int32 Circle, UInt16 posi_mode);
    // Chuyển động nội suy hình chữ nhật (Unit) (Áp dụng thẻ EtherCAT, RTEX, dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_rectangle_move_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_rectangle_move_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] TargetPos, double[] MaskPos, Int32 Count, UInt16 rect_mode, UInt16 posi_mode);

    //---------------------- Chuyển động PVT (Vị trí - Vận tốc - Thời gian) ---------------------------
    // Chuyển động PVT phiên bản cũ (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_PvtTable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_PvtTable(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, Int32[] pPos, double[] pVel);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_PtsTable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_PtsTable(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, Int32[] pPos, double[] pPercent);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_PvtsTable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_PvtsTable(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, Int32[] pPos, double velBegin, double velEnd);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_PttTable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_PttTable(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, int[] pPos);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_PvtMove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_PvtMove(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList);
    // Thêm vào vùng đệm PVT
    [DllImport("LTDMC.dll")]
    public static extern short dmc_PttTable_add(UInt16 CardNo, UInt16 iaxis, UInt16 count, double[] pTime, long[] pPos);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_PtsTable_add(UInt16 CardNo, UInt16 iaxis, UInt16 count, double[] pTime, long[] pPos, double[] pPercent);
    // Đọc không gian PVT còn lại
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pvt_get_remain_space(UInt16 CardNo, UInt16 iaxis);
    // Chuyển động PVT quy hoạch mới cho thẻ Bus, áp dụng cho thẻ EtherCAT
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pvt_table_unit(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, double[] pPos, double[] pVel);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pts_table_unit(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, double[] pPos, double[] pPercent);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pvts_table_unit(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, double[] pPos, double velBegin, double velEnd);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_ptt_table_unit(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, double[] pPos);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pvt_move(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList);
    // Loại khác (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_SetGearProfile(UInt16 CardNo, UInt16 axis, UInt16 MasterType, UInt16 MasterIndex, Int32 MasterEven, Int32 SlaveEven, UInt32 MasterSlope);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_GetGearProfile(UInt16 CardNo, UInt16 axis, ref UInt16 MasterType, ref UInt16 MasterIndex, ref UInt32 MasterEven, ref UInt32 SlaveEven, ref UInt32 MasterSlope);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_GearMove(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList);

    //-------------------- Chuyển động về gốc (Homing) ---------------------
    // Thiết lập/Đọc logic tín hiệu HOME (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_home_pin_logic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_home_pin_logic(UInt16 CardNo, UInt16 axis, UInt16 org_logic, double filter);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_home_pin_logic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_home_pin_logic(UInt16 CardNo, UInt16 axis, ref UInt16 org_logic, ref double filter);
    // Thiết lập/Đọc chế độ về gốc của trục chỉ định (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_homemode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_homemode(UInt16 CardNo, UInt16 axis, UInt16 home_dir, double vel, UInt16 mode, UInt16 EZ_count);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_homemode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_homemode(UInt16 CardNo, UInt16 axis, ref UInt16 home_dir, ref double vel, ref UInt16 home_mode, ref UInt16 EZ_count);
    // Thiết lập về gốc có quay lại khi chạm giới hạn EL hay không (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_home_el_return(UInt16 CardNo, UInt16 axis, UInt16 enable);
    // Đọc tham số cho phép quay lại khi chạm EL khi về gốc (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_home_el_return(UInt16 CardNo, UInt16 axis, ref UInt16 enable);
    // Kích hoạt chuyển động về gốc (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_home_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_home_move(UInt16 CardNo, UInt16 axis);
    // Thiết lập/Đọc tham số vận tốc về gốc (Áp dụng thẻ Rtex Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_home_profile_unit(ushort CardNo, ushort axis, double Low_Vel, double High_Vel, double Tacc, double Tdec);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_home_profile_unit(ushort CardNo, ushort axis, ref double Low_Vel, ref double High_Vel, ref double Tacc, ref double Tdec);
    // Đọc trạng thái thực hiện về gốc (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_home_result(UInt16 CardNo, UInt16 axis, ref UInt16 state);
    // Thiết lập/Đọc độ lệch vị trí gốc và chế độ xóa gốc (Áp dụng thẻ DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_home_position_unit(UInt16 CardNo, UInt16 axis, UInt16 enable, double position);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_home_position_unit(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref double position);
    // (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_el_home(UInt16 CardNo, UInt16 axis, UInt16 mode);
    // Hàm chế độ dịch chuyển gốc (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_home_shift_param(UInt16 CardNo, UInt16 axis, UInt16 pos_clear_mode, double ShiftValue);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_home_shift_param(UInt16 CardNo, UInt16 axis, ref UInt16 pos_clear_mode, ref double ShiftValue);
    // Thiết lập độ lệch vị trí gốc và chế độ dịch chuyển (Áp dụng dòng DMC3000 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_home_position(UInt16 CardNo, UInt16 axis, UInt16 enable, double position);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_home_position(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref double position);
    // Thiết lập khoảng cách giới hạn mềm khi về gốc (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_home_soft_limit(UInt16 CardNo, UInt16 Axis, Int32 N_limit, Int32 P_limit);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_home_soft_limit(UInt16 CardNo, UInt16 Axis, ref Int32 N_limit, ref Int32 P_limit);

    //-------------------- Chốt vị trí gốc (Origin Latch) -------------------
    // Thiết lập/Đọc chế độ chốt EZ (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_homelatch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_homelatch_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 logic, UInt16 source);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_homelatch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_homelatch_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 logic, ref UInt16 source);
    // Đọc cờ chốt gốc (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_homelatch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_homelatch_flag(UInt16 CardNo, UInt16 axis);
    // Xóa cờ chốt gốc (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_reset_homelatch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_reset_homelatch_flag(UInt16 CardNo, UInt16 axis);
    // Đọc giá trị chốt gốc (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_homelatch_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 dmc_get_homelatch_value(UInt16 CardNo, UInt16 axis);
    // Đọc giá trị chốt gốc (unit) (Áp dụng dòng DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_homelatch_value_unit(UInt16 CardNo, UInt16 axis, ref double pos);

    //-------------------- Chốt EZ -------------------
    // Thiết lập/Đọc chế độ chốt EZ (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_ezlatch_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 logic, UInt16 source);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_ezlatch_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 logic, ref UInt16 source);
    // Đọc cờ chốt EZ (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_ezlatch_flag(UInt16 CardNo, UInt16 axis);
    // Xóa cờ chốt EZ (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_reset_ezlatch_flag(UInt16 CardNo, UInt16 axis);
    // Đọc giá trị chốt EZ (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern Int32 dmc_get_ezlatch_value(UInt16 CardNo, UInt16 axis);
    // Đọc giá trị chốt EZ (unit) (Áp dụng dòng DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_ezlatch_value_unit(UInt16 CardNo, UInt16 axis, ref double pos);

    //-------------------- Chuyển động tay quay (Handwheel) ---------------------
    // Thiết lập/Đọc kênh tay quay (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_handwheel_channel", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_handwheel_channel(UInt16 CardNo, UInt16 index);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_handwheel_channel", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_handwheel_channel(UInt16 CardNo, ref UInt16 index);
    // Thiết lập/Đọc chế độ làm việc tín hiệu xung tay quay đơn trục (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_handwheel_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_handwheel_inmode(UInt16 CardNo, UInt16 axis, UInt16 inmode, Int32 multi, double vh);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_handwheel_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_handwheel_inmode(UInt16 CardNo, UInt16 axis, ref UInt16 inmode, ref Int32 multi, ref double vh);
    // Thiết lập/Đọc chế độ làm việc tay quay đơn trục, tỷ lệ kiểu số thực (Áp dụng dòng DMC5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_handwheel_inmode_decimals", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_handwheel_inmode_decimals(UInt16 CardNo, UInt16 axis, UInt16 inmode, double multi, double vh);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_handwheel_inmode_decimals", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_handwheel_inmode_decimals(UInt16 CardNo, UInt16 axis, ref UInt16 inmode, ref double multi, ref double vh);
    // Thiết lập/Đọc chế độ làm việc tín hiệu xung tay quay đa trục (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_handwheel_inmode_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_handwheel_inmode_extern(UInt16 CardNo, UInt16 inmode, UInt16 AxisNum, UInt16[] AxisList, Int32[] multi);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_handwheel_inmode_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_handwheel_inmode_extern(UInt16 CardNo, ref UInt16 inmode, ref UInt16 AxisNum, UInt16[] AxisList, Int32[] multi);
    // Thiết lập/Đọc chế độ làm việc tay quay đa trục, tỷ lệ kiểu số thực (Áp dụng dòng DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_handwheel_inmode_extern_decimals(UInt16 CardNo, UInt16 inmode, UInt16 AxisNum, UInt16[] AxisList, double[] multi);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_handwheel_inmode_extern_decimals(UInt16 CardNo, ref UInt16 inmode, ref UInt16 AxisNum, UInt16[] AxisList, double[] multi);
    // Kích hoạt chuyển động tay quay (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_handwheel_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_handwheel_move(UInt16 CardNo, UInt16 axis);
    // Chuyển động tay quay, chế độ tay quay mới của thẻ Bus (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_handwheel_set_axislist(UInt16 CardNo, UInt16 AxisSelIndex, UInt16 AxisNum, UInt16[] AxisList);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_handwheel_get_axislist(UInt16 CardNo, UInt16 AxisSelIndex, ref UInt16 AxisNum, UInt16[] AxisList);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_handwheel_set_ratiolist(UInt16 CardNo, UInt16 AxisSelIndex, UInt16 StartRatioIndex, UInt16 RatioSelNum, double[] RatioList);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_handwheel_get_ratiolist(UInt16 CardNo, UInt16 AxisSelIndex, UInt16 StartRatioIndex, UInt16 RatioSelNum, double[] RatioList);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_handwheel_set_mode(UInt16 CardNo, UInt16 InMode, UInt16 IfHardEnable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_handwheel_get_mode(UInt16 CardNo, ref UInt16 InMode, ref UInt16 IfHardEnable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_handwheel_set_index(UInt16 CardNo, UInt16 AxisSelIndex, UInt16 RatioSelIndex);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_handwheel_get_index(UInt16 CardNo, ref UInt16 AxisSelIndex, ref UInt16 RatioSelIndex);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_handwheel_stop(UInt16 CardNo);

    //------------------------- Chốt vị trí tốc độ cao (High-speed Latch) -------------------
    // Thiết lập/Đọc tín hiệu LTC của trục chỉ định (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_ltc_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_ltc_mode(UInt16 CardNo, UInt16 axis, UInt16 ltc_logic, UInt16 ltc_mode, Double filter);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_ltc_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_ltc_mode(UInt16 CardNo, UInt16 axis, ref UInt16 ltc_logic, ref UInt16 ltc_mode, ref Double filter);
    // Thiết lập chế độ chốt khi đọc được (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_latch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_latch_mode(UInt16 CardNo, UInt16 axis, UInt16 all_enable, UInt16 latch_source, UInt16 triger_chunnel);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_latch_mode(UInt16 CardNo, UInt16 axis, ref UInt16 all_enable, ref UInt16 latch_source, ref UInt16 triger_chunnel);
    // Đọc giá trị chốt của bộ đếm (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 dmc_get_latch_value(UInt16 CardNo, UInt16 axis);
    // Đọc giá trị chốt của bộ đếm unit (Áp dụng dòng DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_latch_value_unit(UInt16 CardNo, UInt16 axis, ref double pos_by_mm);
    // Đọc cờ chốt (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_latch_flag(UInt16 CardNo, UInt16 axis);
    // Reset cờ chốt (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_reset_latch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_reset_latch_flag(UInt16 CardNo, UInt16 axis);
    // Lấy giá trị theo chỉ số Index (Áp dụng dòng DMC3000 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_value_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 dmc_get_latch_value_extern(UInt16 CardNo, UInt16 axis, UInt16 Index);
    // Chốt tốc độ cao (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_latch_value_extern_unit(UInt16 CardNo, UInt16 axis, UInt16 index, ref double pos_by_mm);
    // Đọc số lượng chốt (Áp dụng dòng DMC3000 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_flag_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_latch_flag_extern(UInt16 CardNo, UInt16 axis);
    // Thiết lập/Đọc đầu ra đảo ngược LTC (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_SetLtcOutMode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_SetLtcOutMode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 bitno);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_GetLtcOutMode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_GetLtcOutMode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 bitno);
    // Thời gian dừng trễ khi kích hoạt cổng LTC (đơn vị us) (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_latch_stop_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_latch_stop_time(UInt16 CardNo, UInt16 axis, Int32 time);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_stop_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_latch_stop_time(UInt16 CardNo, UInt16 axis, ref Int32 time);
    // Thiết lập/Đọc cấu hình trục dừng trễ kích hoạt cổng LTC (Áp dụng thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_latch_stop_axis(ushort CardNo, ushort latch, ushort num, ushort[] axislist);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_latch_stop_axis(ushort CardNo, ushort latch, ref ushort num, ushort[] axislist);

    //---------------------- Chốt vị trí tốc độ cao - Thẻ Bus ---------------------------
    // Cấu hình chốt: chế độ 0-chốt đơn, 1-chốt liên tục; cạnh 0-cạnh xuống, 1-cạnh lên, 2-cả hai; lọc us (Áp dụng cho tất cả thẻ Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_ltc_set_mode(ushort CardNo, ushort latch, ushort ltc_mode, ushort ltc_logic, double filter);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_ltc_get_mode(ushort CardNo, ushort latch, ref ushort ltc_mode, ref ushort ltc_logic, ref double filter);
    // Cấu hình nguồn chốt: 0-vị trí lệnh, 1-vị trí phản hồi encoder (Áp dụng cho tất cả thẻ Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_ltc_set_source(ushort CardNo, ushort latch, ushort axis, ushort ltc_source);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_ltc_get_source(ushort CardNo, ushort latch, ushort axis, ref ushort ltc_source);
    // Reset chốt (Áp dụng cho tất cả thẻ Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_ltc_reset(ushort CardNo, ushort latch);
    // Đọc số lượng chốt (Áp dụng cho tất cả thẻ Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_ltc_get_number(ushort CardNo, ushort latch, ushort axis, ref int number);
    // Đọc giá trị chốt (Áp dụng cho tất cả thẻ Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_ltc_get_value_unit(ushort CardNo, ushort latch, ushort axis, ref double value);

    //----------------------- Chốt mềm (Soft Latch) - Tất cả các thẻ ---------------------------------
    // Cấu hình chốt mềm: 0-đơn, 1-liên tục; cạnh 0-xuống, 1-lên, 2-cả hai; lọc us (Áp dụng dòng DMC5X10/3000 Pulse, thẻ Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_softltc_set_mode(ushort ConnectNo, ushort latch, ushort ltc_enable, ushort ltc_mode, ushort ltc_inbit, ushort ltc_logic, double filter);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_softltc_get_mode(ushort ConnectNo, ushort latch, ref ushort ltc_enable, ref ushort ltc_mode, ref ushort ltc_inbit, ref ushort ltc_logic, ref double filter);
    // Cấu hình nguồn chốt mềm: 0-lệnh, 1-phản hồi encoder (Áp dụng dòng DMC5X10/3000 Pulse, thẻ Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_softltc_set_source(ushort ConnectNo, ushort latch, ushort axis, ushort ltc_source);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_softltc_get_source(ushort ConnectNo, ushort latch, ushort axis, ref ushort ltc_source);
    // Reset chốt mềm (Áp dụng dòng DMC5X10/3000 Pulse, thẻ Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_softltc_reset(ushort ConnectNo, ushort latch);
    // Đọc số lượng chốt mềm (Áp dụng dòng DMC5X10/3000 Pulse, thẻ Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_softltc_get_number(ushort ConnectNo, ushort latch, ushort axis, ref int number);
    // Đọc giá trị chốt mềm (Áp dụng dòng DMC5X10 Pulse, tất cả thẻ Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_softltc_get_value_unit(ushort ConnectNo, ushort latch, ushort axis, ref double value);

    //---------------------- So sánh vị trí tốc độ thấp đơn trục -----------------------
    // Cấu hình/Đọc bộ so sánh (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_set_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_set_config(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 cmp_source);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_get_config(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 cmp_source);
    // Xóa tất cả các điểm so sánh (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_clear_points", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_clear_points(UInt16 CardNo, UInt16 axis);
    // Thêm điểm so sánh (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_add_point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_add_point(UInt16 CardNo, UInt16 axis, int pos, UInt16 dir, UInt16 action, UInt32 actpara);
    // Thêm điểm so sánh unit (Áp dụng thẻ DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_compare_add_point_unit(UInt16 CardNo, UInt16 cmp, double pos, UInt16 dir, UInt16 action, UInt32 actpara);
    // Thêm điểm so sánh chu kỳ (Áp dụng thẻ E3032/R3032)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_compare_add_point_cycle(UInt16 CardNo, UInt16 cmp, Int32 pos, UInt16 dir, UInt32 bitno, UInt32 cycle, UInt16 level);
    // Thêm điểm so sánh chu kỳ unit (Áp dụng thẻ E5032)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_compare_add_point_cycle_unit(UInt16 CardNo, UInt16 cmp, double pos, UInt16 dir, UInt32 bitno, UInt32 cycle, UInt16 level);
    // Đọc điểm so sánh hiện tại (Áp dụng cho tất cả các thẻ Pulse, thẻ Rtex Bus, E3032)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_current_point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_get_current_point(UInt16 CardNo, UInt16 axis, ref Int32 pos);
    // Đọc điểm so sánh hiện tại unit (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_compare_get_current_point_unit(UInt16 CardNo, UInt16 cmp, ref double pos);
    // Truy vấn các điểm đã so sánh (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_points_runned", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_get_points_runned(UInt16 CardNo, UInt16 axis, ref Int32 pointNum);
    // Truy vấn số lượng điểm so sánh có thể thêm (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_points_remained", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_get_points_remained(UInt16 CardNo, UInt16 axis, ref Int32 pointNum);

    //------------------- So sánh vị trí tốc độ thấp 2D -----------------------
    // Cấu hình/Đọc bộ so sánh (Áp dụng cho tất cả các thẻ Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_set_config_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_set_config_extern(UInt16 CardNo, UInt16 enable, UInt16 cmp_source);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_config_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_get_config_extern(UInt16 CardNo, ref UInt16 enable, ref UInt16 cmp_source);
    // Xóa tất cả điểm so sánh (Áp dụng cho tất cả các thẻ Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_clear_points_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_clear_points_extern(UInt16 CardNo);
    // Thêm điểm so sánh vị trí hai trục (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_add_point_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_add_point_extern(UInt16 CardNo, UInt16[] axis, Int32[] pos, UInt16[] dir, UInt16 action, UInt32 actpara);
    // Đọc điểm so sánh hiện tại (Áp dụng cho tất cả các thẻ Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_current_point_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_get_current_point_extern(UInt16 CardNo, Int32[] pos);
    // Đọc điểm so sánh hiện tại unit (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_compare_get_current_point_extern_unit(UInt16 CardNo, double[] pos);
    // Thêm điểm so sánh vị trí hai trục (Áp dụng thẻ DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_compare_add_point_extern_unit(UInt16 CardNo, UInt16[] axis, double[] pos, UInt16[] dir, UInt16 action, UInt32 actpara);
    // Thêm điểm so sánh chu kỳ vị trí 2D tốc độ thấp (Áp dụng thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_compare_add_point_cycle_2d(ushort CardNo, ushort[] axis, double[] pos, ushort[] dir, uint bitno, uint cycle, ushort level);
    // Truy vấn điểm đã so sánh (Áp dụng cho tất cả các thẻ Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_points_runned_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_get_points_runned_extern(UInt16 CardNo, ref Int32 pointNum);
    // Truy vấn số điểm có thể thêm (Áp dụng cho tất cả các thẻ Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_points_remained_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_compare_get_points_remained_extern(UInt16 CardNo, ref Int32 pointNum);
    // So sánh vị trí đa nhóm (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_compare_set_config_multi(UInt16 CardNo, UInt16 queue, UInt16 enable, UInt16 axis, UInt16 cmp_source);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_compare_get_config_multi(UInt16 CardNo, UInt16 queue, ref UInt16 enable, ref UInt16 axis, ref UInt16 cmp_source);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_compare_add_point_multi(UInt16 CardNo, UInt16 cmp, Int32 pos, UInt16 dir, UInt16 action, UInt32 actpara, double times);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_compare_add_point_multi_unit(UInt16 CardNo, UInt16 cmp, double pos, UInt16 dir, UInt16 action, UInt32 actpara, double times);

    //----------- So sánh vị trí tốc độ cao đơn trục -----------------------
    // Thiết lập/Đọc chế độ so sánh tốc độ cao (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_set_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_hcmp_set_mode(UInt16 CardNo, UInt16 hcmp, UInt16 cmp_enable);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_get_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_hcmp_get_mode(UInt16 CardNo, UInt16 hcmp, ref UInt16 cmp_enable);
    // Thiết lập tham số so sánh tốc độ cao (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_set_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_hcmp_set_config(UInt16 CardNo, UInt16 hcmp, UInt16 axis, UInt16 cmp_source, UInt16 cmp_logic, Int32 time);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_get_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_hcmp_get_config(UInt16 CardNo, UInt16 hcmp, ref UInt16 axis, ref UInt16 cmp_source, ref UInt16 cmp_logic, ref Int32 time);
    // Mở rộng chế độ so sánh tốc độ cao (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_set_config_extern(UInt16 CardNo, UInt16 hcmp, UInt16 axis, UInt16 cmp_source, UInt16 cmp_logic, UInt16 cmp_mode, Int32 dist, Int32 time);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_get_config_extern(UInt16 CardNo, UInt16 hcmp, ref UInt16 axis, ref UInt16 cmp_source, ref UInt16 cmp_logic, ref UInt16 cmp_mode, ref Int32 dist, ref Int32 time);
    // Thêm điểm so sánh (Áp dụng cho tất cả thẻ Pulse, thẻ Bus E3032, R3032)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_add_point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_hcmp_add_point(UInt16 CardNo, UInt16 hcmp, Int32 cmp_pos);
    // Thêm điểm so sánh unit (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_add_point_unit(UInt16 CardNo, UInt16 hcmp, double cmp_pos);
    // Thiết lập/Đọc tham số chế độ tuyến tính (Áp dụng cho tất cả thẻ Pulse, thẻ E3032, R3032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_set_liner", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_hcmp_set_liner(UInt16 CardNo, UInt16 hcmp, Int32 Increment, Int32 Count);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_get_liner", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_hcmp_get_liner(UInt16 CardNo, UInt16 hcmp, ref Int32 Increment, ref Int32 Count);
    // Thiết lập tham số chế độ tuyến tính (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_set_liner_unit(UInt16 CardNo, UInt16 hcmp, double Increment, Int32 Count);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_get_liner_unit(UInt16 CardNo, UInt16 hcmp, ref double Increment, ref Int32 Count);
    // Đọc trạng thái so sánh tốc độ cao (Áp dụng cho tất cả thẻ Pulse, thẻ E3032, R3032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_get_current_state", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_hcmp_get_current_state(UInt16 CardNo, UInt16 hcmp, ref Int32 remained_points, ref Int32 current_point, ref Int32 runned_points);
    // Đọc trạng thái so sánh tốc độ cao (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_get_current_state_unit(UInt16 CardNo, UInt16 hcmp, ref Int32 remained_points, ref double current_point, ref Int32 runned_points);
    // Xóa điểm so sánh (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_clear_points", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_hcmp_clear_points(UInt16 CardNo, UInt16 hcmp);
    // Đọc mức điện của cổng CMP chỉ định (Dành riêng)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_cmp_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_read_cmp_pin(UInt16 CardNo, UInt16 hcmp);
    // Điều khiển đầu ra cổng cmp (Dành riêng)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_write_cmp_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_write_cmp_pin(UInt16 CardNo, UInt16 hcmp, UInt16 on_off);
    // 1. Kích hoạt chế độ đệm để thêm vị trí so sánh (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_fifo_set_mode(UInt16 CardNo, UInt16 hcmp, UInt16 fifo_mode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_fifo_get_mode(UInt16 CardNo, UInt16 hcmp, ref UInt16 fifo_mode);
    // 2. Đọc trạng thái đệm còn lại, máy chủ dùng hàm này để quyết định có tiếp tục thêm vị trí (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_fifo_get_state(UInt16 CardNo, UInt16 hcmp, ref long remained_points);
    // 3. Thêm điểm so sánh theo lô dạng mảng (Áp dụng dòng DMC5000 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_fifo_add_point_unit(UInt16 CardNo, UInt16 hcmp, UInt16 num, double[] cmp_pos);
    // 4. Xóa vị trí so sánh, đồng thời xóa vị trí trong FPGA (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_fifo_clear_points(UInt16 CardNo, UInt16 hcmp);
    // Thêm mảng lớn, sẽ bị khóa một lúc cho đến khi thêm xong (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_fifo_add_table(UInt16 CardNo, UInt16 hcmp, UInt16 num, double[] cmp_pos);
    // So sánh tốc độ cao 1D, chế độ hàng đợi thêm điểm so sánh liên quan hướng chuyển động, thêm ít dữ liệu (Áp dụng dòng DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_fifo_add_point_dir_unit(ushort CardNo, ushort hcmp, ushort num, double[] cmp_pos, uint dir);
    // So sánh tốc độ cao 1D, chế độ hàng đợi thêm điểm so sánh liên quan hướng chuyển động, thêm nhiều dữ liệu (Áp dụng dòng DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_fifo_add_table_dir(ushort CardNo, ushort hcmp, ushort num, double[] cmp_pos, uint dir);

    //----------- So sánh vị trí tốc độ cao 2D -----------------------
    // Thiết lập/Đọc kích hoạt so sánh tốc độ cao (Áp dụng cho tất cả thẻ Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_set_enable(UInt16 CardNo, UInt16 hcmp, UInt16 cmp_enable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_get_enable(UInt16 CardNo, UInt16 hcmp, ref UInt16 cmp_enable);
    // Cấu hình/Đọc bộ so sánh tốc độ cao 2D (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_set_config(UInt16 CardNo, UInt16 hcmp, UInt16 cmp_mode, UInt16 x_axis, UInt16 x_cmp_source, UInt16 y_axis, UInt16 y_cmp_source, Int32 error, UInt16 cmp_logic, Int32 time, UInt16 pwm_enable, double duty, Int32 freq, UInt16 port_sel, UInt16 pwm_number);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_get_config(UInt16 CardNo, UInt16 hcmp, ref UInt16 cmp_mode, ref UInt16 x_axis, ref UInt16 x_cmp_source, ref UInt16 y_axis, ref UInt16 y_cmp_source, ref Int32 error, ref UInt16 cmp_logic, ref Int32 time, ref UInt16 pwm_enable, ref double duty, ref Int32 freq, ref UInt16 port_sel, ref UInt16 pwm_number);
    // Cấu hình/Đọc bộ so sánh tốc độ cao 2D (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_set_config_unit(UInt16 CardNo, UInt16 hcmp, UInt16 cmp_mode, UInt16 x_axis, UInt16 x_cmp_source, double x_cmp_error, UInt16 y_axis, UInt16 y_cmp_source, double y_cmp_error, UInt16 cmp_logic, int time);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_get_config_unit(UInt16 CardNo, UInt16 hcmp, ref UInt16 cmp_mode, ref UInt16 x_axis, ref UInt16 x_cmp_source, ref double x_cmp_error, ref UInt16 y_axis, ref UInt16 y_cmp_source, ref double y_cmp_error, ref UInt16 cmp_logic, ref int time);
    // Thêm điểm so sánh vị trí tốc độ cao 2D (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_add_point(UInt16 CardNo, UInt16 hcmp, Int32 x_cmp_pos, Int32 y_cmp_pos);
    // Thêm điểm so sánh vị trí tốc độ cao 2D unit (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_add_point_unit(UInt16 CardNo, UInt16 hcmp, double x_cmp_pos, double y_cmp_pos, UInt16 cmp_outbit);
    // Đọc tham số so sánh tốc độ cao 2D (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_get_current_state(UInt16 CardNo, UInt16 hcmp, ref Int32 remained_points, ref Int32 x_current_point, ref Int32 y_current_point, ref Int32 runned_points, ref UInt16 current_state);
    // Đọc tham số so sánh tốc độ cao 2D (Áp dụng dòng DMC5X10 Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_get_current_state_unit(UInt16 CardNo, UInt16 hcmp, ref int remained_points, ref double x_current_point, ref double y_current_point, ref int runned_points, ref UInt16 current_state, ref UInt16 current_outbit);
    // Xóa điểm so sánh tốc độ cao 2D (Áp dụng cho tất cả thẻ Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_clear_points(UInt16 CardNo, UInt16 hcmp);
    // Cưỡng bức đầu ra so sánh 2D (Áp dụng cho tất cả thẻ Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_force_output(UInt16 CardNo, UInt16 hcmp, UInt16 cmp_outbit);
    // Cấu hình/Đọc chế độ đầu ra PWM so sánh 2D (Áp dụng dòng DMC5X10 Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_set_pwmoutput(UInt16 CardNo, UInt16 hcmp, UInt16 pwm_enable, double duty, double freq, UInt16 pwm_number);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_2d_get_pwmoutput(UInt16 CardNo, UInt16 hcmp, ref UInt16 pwm_enable, ref double duty, ref double freq, ref UInt16 pwm_number);

    //------------------------ IO thông dụng -----------------------
    // Đọc trạng thái cổng vào (Input) (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_inbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_read_inbit(UInt16 CardNo, UInt16 bitno);
    // Thiết lập trạng thái cổng ra (Output) (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_write_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_write_outbit(UInt16 CardNo, UInt16 bitno, UInt16 on_off);
    // Đọc trạng thái cổng ra (Output) (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_read_outbit(UInt16 CardNo, UInt16 bitno);
    // Đọc giá trị cổng vào Port (32 bit) (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_inport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 dmc_read_inport(UInt16 CardNo, UInt16 portno);
    // Đọc giá trị cổng ra Port (32 bit) (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 dmc_read_outport(UInt16 CardNo, UInt16 portno);
    // Thiết lập giá trị cho tất cả cổng ra Port (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_write_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_write_outport(UInt16 CardNo, UInt16 portno, UInt32 outport_val);
    // Thiết lập giá trị cổng ra thông dụng (Dành riêng)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_write_outport_16X", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_write_outport_16X(UInt16 CardNo, UInt16 portno, UInt32 outport_val);
    //--------------------------- IO thông dụng có kiểm tra giá trị trả về ----------------------
    // Đọc trạng thái cổng vào (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_read_inbit_ex(ushort CardNo, ushort bitno, ref ushort state);
    // Đọc trạng thái cổng ra (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_read_outbit_ex(ushort CardNo, ushort bitno, ref ushort state);
    // Đọc giá trị cổng vào Port (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_read_inport_ex(ushort CardNo, ushort portno, ref UInt32 state);
    // Đọc giá trị cổng ra Port (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_read_outport_ex(ushort CardNo, ushort portno, ref UInt32 state);

    // Thiết lập/Đọc quan hệ ánh xạ IO ảo (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_io_map_virtual", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_io_map_virtual(UInt16 CardNo, UInt16 bitno, UInt16 MapIoType, UInt16 MapIoIndex, double Filter);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_io_map_virtual", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_io_map_virtual(UInt16 CardNo, UInt16 bitno, ref UInt16 MapIoType, ref UInt16 MapIoIndex, ref double Filter);
    // Đọc trạng thái cổng vào ảo (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_inbit_virtual", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_read_inbit_virtual(UInt16 CardNo, UInt16 bitno);
    // Đảo ngược IO trễ (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_reverse_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_reverse_outbit(UInt16 CardNo, UInt16 bitno, double reverse_time);
    // Thiết lập/Đọc chế độ đếm IO (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_io_count_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_io_count_mode(UInt16 CardNo, UInt16 bitno, UInt16 mode, double filter_time);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_io_count_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_io_count_mode(UInt16 CardNo, UInt16 bitno, ref UInt16 mode, ref double filter_time);
    // Thiết lập giá trị đếm IO (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_io_count_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_io_count_value(UInt16 CardNo, UInt16 bitno, UInt32 CountValue);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_io_count_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_io_count_value(UInt16 CardNo, UInt16 bitno, ref UInt32 CountValue);

    //----------------------- IO chuyên dụng - Chỉ dành cho thẻ Pulse -------------------------
    // Thiết lập/Đọc ánh xạ IO trục (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_axis_io_map", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_axis_io_map(UInt16 CardNo, UInt16 Axis, UInt16 IoType, UInt16 MapIoType, UInt16 MapIoIndex, double Filter);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_axis_io_map", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_axis_io_map(UInt16 CardNo, UInt16 Axis, UInt16 IoType, ref UInt16 MapIoType, ref UInt16 MapIoIndex, ref double Filter);
    // Thiết lập thời gian lọc cho tất cả IO chuyên dụng (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_special_input_filter", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_special_input_filter(UInt16 CardNo, double Filter);
    // Cấu hình tín hiệu giảm tốc khi về gốc (Dành cho DMC3410)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_sd_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_sd_mode(UInt16 CardNo, UInt16 axis, UInt16 sd_logic, UInt16 sd_mode);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_sd_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_sd_mode(UInt16 CardNo, UInt16 axis, ref UInt16 sd_logic, ref UInt16 sd_mode);
    // Thiết lập/Đọc tín hiệu INP (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_inp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_inp_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 inp_logic);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_inp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_inp_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 inp_logic);
    // Thiết lập/Đọc tín hiệu RDY (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_rdy_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 rdy_logic);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_rdy_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 rdy_logic);
    // Thiết lập/Đọc tín hiệu ERC (Dành riêng)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_erc_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_erc_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 erc_logic, UInt16 erc_width, UInt16 erc_off_time);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_erc_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_erc_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 erc_logic, ref UInt16 erc_width, ref UInt16 erc_off_time);
    // Thiết lập/Đọc tín hiệu ALM (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_alm_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_alm_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 alm_logic, UInt16 alm_action);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_alm_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_alm_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 alm_logic, ref UInt16 alm_action);
    // Thiết lập/Đọc tín hiệu EZ (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_ez_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_ez_mode(UInt16 CardNo, UInt16 axis, UInt16 ez_logic, UInt16 ez_mode, double filter);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_ez_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_ez_mode(UInt16 CardNo, UInt16 axis, ref UInt16 ez_logic, ref UInt16 ez_mode, ref double filter);
    // Xuất/Đọc tín hiệu SEVON (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_write_sevon_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_write_sevon_pin(UInt16 CardNo, UInt16 axis, UInt16 on_off);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_sevon_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_read_sevon_pin(UInt16 CardNo, UInt16 axis);
    // Điều khiển đầu ra tín hiệu ERC (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_write_erc_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_write_erc_pin(UInt16 CardNo, UInt16 axis, UInt16 sel);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_erc_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_read_erc_pin(UInt16 CardNo, UInt16 axis);
    // Đọc trạng thái RDY (Áp dụng cho tất cả các thẻ Pul se)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_rdy_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_read_rdy_pin(UInt16 CardNo, UInt16 axis);
    // Xuất tín hiệu reset Servo (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_write_sevrst_pin(UInt16 CardNo, UInt16 axis, UInt16 on_off);
    // Đọc tín hiệu reset Servo (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_read_sevrst_pin(UInt16 CardNo, UInt16 axis);

    //--------------------- Bộ đếm Encoder - Thẻ Pulse ---------------------
    // Thiết lập/Đọc chế độ đếm của bộ đếm (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_counter_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_counter_inmode(UInt16 CardNo, UInt16 axis, UInt16 mode);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_counter_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_counter_inmode(UInt16 CardNo, UInt16 axis, ref UInt16 mode);
    // Giá trị bộ đếm (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 dmc_get_encoder(UInt16 CardNo, UInt16 axis);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_encoder(UInt16 CardNo, UInt16 axis, Int32 encoder_value);
    // Giá trị bộ đếm unit (Áp dụng dòng DMC5000/5X10 Pulse, tất cả thẻ Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_encoder_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_encoder_unit(UInt16 CardNo, UInt16 axis, double pos);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_encoder_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_encoder_unit(UInt16 CardNo, UInt16 axis, ref double pos);

    //--------------------- Bộ đếm phụ - Thẻ Bus ---------------------
    // Bộ đếm tay quay (Dùng cho dmc_set_extra_encoder)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_handwheel_encoder(ushort CardNo, ushort channel, int pos);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_handwheel_encoder(ushort CardNo, ushort channel, ref int pos);
    // Thiết lập chế độ bộ đếm phụ (Áp dụng cho tất cả thẻ Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_extra_encoder_mode(ushort CardNo, ushort channel, ushort inmode, ushort multi);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_extra_encoder_mode(ushort CardNo, ushort channel, ref ushort inmode, ref ushort multi);
    // Thiết lập giá trị bộ đếm phụ (Áp dụng cho tất cả thẻ Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_extra_encoder(ushort CardNo, ushort channel, int pos);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_extra_encoder(ushort CardNo, ushort channel, ref int pos);

    //--------------------- Điều khiển đếm vị trí ---------------------
    // Vị trí hiện tại unit (Áp dụng dòng DMC5000/5X10 Pulse, tất cả thẻ Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_position_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_position_unit(UInt16 CardNo, UInt16 axis, double pos);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_position_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_position_unit(UInt16 CardNo, UInt16 axis, ref double pos);
    // Vị trí hiện tại (Xung) (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 dmc_get_position(UInt16 CardNo, UInt16 axis);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_position(UInt16 CardNo, UInt16 axis, Int32 current_position);

    //-------------------- Trạng thái chuyển động ----------------------
    // Đọc vận tốc hiện tại của trục chỉ định (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_current_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern double dmc_read_current_speed(UInt16 CardNo, UInt16 axis);
    // Đọc vận tốc hiện tại (unit) (Áp dụng dòng DMC5000/5X10 Pulse, tất cả thẻ Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_current_speed_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_read_current_speed_unit(UInt16 CardNo, UInt16 Axis, ref double current_speed);
    // Đọc vận tốc nội suy hiện tại của Card (Áp dụng dòng DMC5X10 Pulse, tất cả thẻ Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_read_vector_speed_unit(UInt16 CardNo, UInt16 Crd, ref double current_speed);
    // Đọc vị trí đích của trục chỉ định (Áp dụng cho tất cả thẻ Pulse, thẻ R3032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_target_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 dmc_get_target_position(UInt16 CardNo, UInt16 axis);
    // Đọc vị trí đích trục chỉ định unit (Áp dụng dòng DMC5X10 Pulse, tất cả thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_target_position_unit(UInt16 CardNo, UInt16 axis, ref double pos);
    // Đọc trạng thái chuyển động trục chỉ định (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_check_done", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_check_done(UInt16 CardNo, UInt16 axis);
    // Đọc trạng thái chuyển động trục chỉ định mở rộng (Áp dụng cho tất cả các thẻ)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_check_done_ex(ushort CardNo, ushort axis, ref ushort state);
    // Trạng thái chuyển động nội suy (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_check_done_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_check_done_multicoor(UInt16 CardNo, UInt16 crd);
    // Đọc trạng thái tín hiệu IO trục chỉ định (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_axis_io_status", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 dmc_axis_io_status(UInt16 CardNo, UInt16 axis);
    // Đọc trạng thái tín hiệu IO trục mở rộng (Áp dụng cho tất cả các thẻ)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_axis_io_status_ex(ushort CardNo, ushort axis, ref uint state);
    // Dừng đơn trục (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_stop(UInt16 CardNo, UInt16 axis, UInt16 stop_mode);
    // Dừng bộ nội suy (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_stop_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_stop_multicoor(UInt16 CardNo, UInt16 crd, UInt16 stop_mode);
    // Dừng khẩn cấp tất cả các trục (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_emg_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_emg_stop(UInt16 CardNo);
    // Trạng thái truyền thông giữa thẻ chính và hộp đấu nối (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_LinkState", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_LinkState(UInt16 CardNo, ref UInt16 State);
    // Đọc chế độ chạy của trục chỉ định (Áp dụng dòng DMC5000/5X10 Pulse, tất cả thẻ Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_axis_run_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_axis_run_mode(UInt16 CardNo, UInt16 axis, ref UInt16 run_mode);
    // Đọc nguyên nhân dừng trục (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_stop_reason", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_stop_reason(UInt16 CardNo, UInt16 axis, ref Int32 StopReason);
    // Xóa nguyên nhân dừng trục (Áp dụng cho tất cả các thẻ Pulse/Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_clear_stop_reason", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_clear_stop_reason(UInt16 CardNo, UInt16 axis);
    // Chức năng Trace (Dùng nội bộ)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_trace", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_trace(UInt16 CardNo, UInt16 axis, UInt16 enable);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_trace", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_trace(UInt16 CardNo, UInt16 axis, ref UInt16 enable);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_trace_data", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_read_trace_data(UInt16 CardNo, UInt16 axis, UInt16 data_option, ref Int32 ReceiveSize, double[] time, double[] data, ref Int32 remain_num);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_trace_start(ushort CardNo, ushort AxisNum, ushort[] AxisList);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_trace_stop(ushort CardNo);

    // Tính toán chiều dài cung tròn (Dự phòng)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_calculate_arclength_center", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_calculate_arclength_center(double[] start_pos, double[] target_pos, double[] cen_pos, UInt16 arc_dir, double circle, ref double ArcLength);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_calculate_arclength_3point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_calculate_arclength_3point(double[] start_pos, double[] mid_pos, double[] target_pos, double circle, ref double ArcLength);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_calculate_arclength_radius", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_calculate_arclength_radius(double[] start_pos, double[] target_pos, double arc_radius, UInt16 arc_dir, double circle, ref double ArcLength);

    //-------------------- Mở rộng CAN-IO ----------------------
    // Mở rộng CAN-IO, hàm giao diện cũ (Dành riêng)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_can_state", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_can_state(UInt16 CardNo, UInt16 NodeNum, UInt16 state, UInt16 Baud);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_can_state", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_can_state(UInt16 CardNo, ref UInt16 NodeNum, ref UInt16 state);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_write_can_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_write_can_outbit(UInt16 CardNo, UInt16 Node, UInt16 bitno, UInt16 on_off);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_can_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_read_can_outbit(UInt16 CardNo, UInt16 Node, UInt16 bitno);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_can_inbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_read_can_inbit(UInt16 CardNo, UInt16 Node, UInt16 bitno);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_write_can_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_write_can_outport(UInt16 CardNo, UInt16 Node, UInt16 PortNo, UInt32 outport_val);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_can_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 dmc_read_can_outport(UInt16 CardNo, UInt16 Node, UInt16 PortNo);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_read_can_inport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 dmc_read_can_inport(UInt16 CardNo, UInt16 Node, UInt16 PortNo);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_can_errcode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_can_errcode(UInt16 CardNo, ref UInt16 Errcode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_can_errcode_extern(UInt16 CardNo, ref UInt16 Errcode, ref UInt16 msg_losed, ref UInt16 emg_msg_num, ref UInt16 lostHeartB, ref UInt16 EmgMsg);
    // Thiết lập đầu ra CAN IO (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_write_outbit(ushort CardNo, ushort NodeID, ushort IoBit, ushort IoValue);
    // Đọc đầu ra CAN IO (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_outbit(ushort CardNo, ushort NodeID, ushort IoBit, ref ushort IoValue);
    // Đọc đầu vào CAN IO (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_inbit(ushort CardNo, ushort NodeID, ushort IoBit, ref ushort IoValue);
    // Thiết lập đầu ra CAN IO 32 bit (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_write_outport(ushort CardNo, ushort NodeID, ushort PortNo, UInt32 IoValue);
    // Đọc đầu ra CAN IO 32 bit (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_outport(ushort CardNo, ushort NodeID, ushort PortNo, ref UInt32 IoValue);
    // Đọc đầu vào CAN IO 32 bit (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_inport(ushort CardNo, ushort NodeID, ushort PortNo, ref UInt32 IoValue);
    //--------------------------- Kiểm tra giá trị trả về CAN IO ----------------------
    // Thiết lập đầu ra CAN IO (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_write_outbit_ex(ushort CardNo, ushort NoteID, ushort IoBit, ushort IoValue, ref ushort state);
    // Đọc đầu ra CAN IO (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_outbit_ex(ushort CardNo, ushort NoteID, ushort IoBit, ref ushort IoValue, ref ushort state);
    // Đọc đầu vào CAN IO (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_inbit_ex(ushort CardNo, ushort NoteID, ushort IoBit, ref ushort IoValue, ref ushort state);
    // Thiết lập đầu ra CAN IO 32 bit (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_write_outport_ex(ushort CardNo, ushort NoteID, ushort portno, UInt32 outport_val, ref ushort state);
    // Đọc đầu ra CAN IO 32 bit (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_outport_ex(ushort CardNo, ushort NoteID, ushort portno, ref UInt32 outport_val, ref ushort state);
    // Đọc đầu vào CAN IO 32 bit (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_inport_ex(ushort CardNo, ushort NoteID, ushort portno, ref UInt32 inport_val, ref ushort state);
    //--------------------------- CAN ADDA ----------------------
    // Lệnh CAN ADDA Thiết lập tham số DA (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_da_output(ushort CardNo, ushort NoteID, ushort channel, double Value);
    // Đọc tham số CAN DA (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_da_output(ushort CardNo, ushort NoteID, ushort channel, ref double Value);
    // Đọc tham số CAN AD (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_ad_input(ushort CardNo, ushort NoteID, ushort channel, ref double Value);
    // Cấu hình chế độ CAN AD (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_ad_mode(ushort CardNo, ushort NoteID, ushort channel, ushort mode, uint buffer_nums);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_ad_mode(ushort CardNo, ushort NoteID, ushort channel, ref ushort mode, uint buffer_nums);
    // Cấu hình chế độ CAN DA (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_da_mode(ushort CardNo, ushort NoteID, ushort channel, ushort mode, uint buffer_nums);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_da_mode(ushort CardNo, ushort NoteID, ushort channel, ref ushort mode, uint buffer_nums);
    // Ghi tham số CAN vào flash (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_write_to_flash(ushort CardNo, ushort PortNum, ushort NodeNum);
    // Kết nối tổng tuyến CAN (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_connect_state(UInt16 CardNo, UInt16 NodeNum, UInt16 state, UInt16 baud);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_connect_state(UInt16 CardNo, ref UInt16 NodeNum, ref UInt16 state);
    //--------------------------- Kiểm tra giá trị trả về CAN ADDA ----------------------
    // Thiết lập tham số CAN DA (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_da_output_ex(ushort CardNo, ushort NoteID, ushort channel, double Value, ref ushort state);
    // Đọc tham số CAN DA (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_da_output_ex(ushort CardNo, ushort NoteID, ushort channel, ref double Value, ref ushort state);
    // Đọc tham số CAN AD (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_ad_input_ex(ushort CardNo, ushort NoteID, ushort channel, ref double Value, ref ushort state);
    // Cấu hình chế độ CAN AD (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_ad_mode_ex(ushort CardNo, ushort NoteID, ushort channel, ushort mode, UInt32 buffer_nums, ref ushort state);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_ad_mode_ex(ushort CardNo, ushort NoteID, ushort channel, ref ushort mode, UInt32 buffer_nums, ref ushort state);
    // Cấu hình chế độ CAN DA (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_da_mode_ex(ushort CardNo, ushort NoteID, ushort channel, ushort mode, UInt32 buffer_nums, ref ushort state);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_da_mode_ex(ushort CardNo, ushort NoteID, ushort channel, ref ushort mode, UInt32 buffer_nums, ref ushort state);
    // Ghi tham số vào flash (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_write_to_flash_ex(ushort CardNo, ushort PortNum, ushort NodeNum, ref ushort state);

    //-------------------- Hàm nội suy liên tục ----------------------
    // Mở danh sách đệm liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_open_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_open_list(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList);
    // Đóng danh sách đệm liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_close_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_close_list(UInt16 CardNo, UInt16 Crd);
    // Reset danh sách đệm liên tục (Dự phòng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_reset_list(UInt16 CardNo, UInt16 Crd);
    // Dừng trong khi nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_stop_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_stop_list(UInt16 CardNo, UInt16 Crd, UInt16 stop_mode);
    // Tạm dừng trong khi nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_pause_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_pause_list(UInt16 CardNo, UInt16 Crd);
    // Bắt đầu nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_start_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_start_list(UInt16 CardNo, UInt16 Crd);
    // Kiểm tra trạng thái chạy nội suy liên tục: 0-chạy, 1-tạm dừng, 2-dừng bất thường, 3-chưa khởi động, 4-rảnh (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_run_state", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_get_run_state(UInt16 CardNo, UInt16 Crd);
    // Kiểm tra trạng thái chạy nội suy liên tục: 0-chạy, 1-dừng (Dự phòng)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_check_done", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_check_done(UInt16 CardNo, UInt16 Crd);
    // Tra cứu số lượng đệm liên tục còn lại (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_remain_space", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 dmc_conti_remain_space(UInt16 CardNo, UInt16 Crd);
    // Đọc mã đánh dấu của đoạn nội suy liên tục hiện tại (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_read_current_mark", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 dmc_conti_read_current_mark(UInt16 CardNo, UInt16 Crd);
    // Chế độ làm mượt góc blend (Áp dụng dòng DMC5000 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_blend", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_set_blend(UInt16 CardNo, UInt16 Crd, UInt16 enable);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_blend", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_get_blend(UInt16 CardNo, UInt16 Crd, ref UInt16 enable);
    // Thiết lập tỷ lệ vận tốc mỗi đoạn (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_override", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_set_override(UInt16 CardNo, UInt16 Crd, double Percent);
    // Thiết lập thay đổi vận tốc động trong nội suy (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_change_speed_ratio", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_change_speed_ratio(UInt16 CardNo, UInt16 Crd, double Percent);
    // Dự đoán trước đoạn thẳng nhỏ (Lookahead) (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_lookahead_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_set_lookahead_mode(UInt16 CardNo, UInt16 Crd, UInt16 enable, Int32 LookaheadSegments, double PathError, double LookaheadAcc);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_lookahead_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_get_lookahead_mode(UInt16 CardNo, UInt16 Crd, ref UInt16 enable, ref Int32 LookaheadSegments, ref double PathError, ref double LookaheadAcc);
    //-------------------- Chức năng IO nội suy liên tục ----------------------
    // Chờ đầu vào IO (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_wait_input", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_wait_input(UInt16 CardNo, UInt16 Crd, UInt16 bitno, UInt16 on_off, double TimeOut, Int32 mark);
    // Xuất IO trễ so với điểm bắt đầu quỹ đạo (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_delay_outbit_to_start", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_delay_outbit_to_start(UInt16 CardNo, UInt16 Crd, UInt16 bitno, UInt16 on_off, double delay_value, UInt16 delay_mode, double ReverseTime);
    // Xuất IO trễ so với điểm kết thúc quỹ đạo (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_delay_outbit_to_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_delay_outbit_to_stop(UInt16 CardNo, UInt16 Crd, UInt16 bitno, UInt16 on_off, double delay_time, double ReverseTime);
    // Xuất IO sớm so với điểm kết thúc quỹ đạo (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_ahead_outbit_to_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_ahead_outbit_to_stop(UInt16 CardNo, UInt16 Crd, UInt16 bitno, UInt16 on_off, double ahead_value, UInt16 ahead_mode, double ReverseTime);
    // Xuất CMP vị trí chính xác trong nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_accurate_outbit_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_accurate_outbit_unit(UInt16 CardNo, UInt16 Crd, UInt16 cmp_no, UInt16 on_off, UInt16 map_axis, double abs_pos, UInt16 posi_source, double ReverseTime);
    // Xuất IO lập tức trong nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_write_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_write_outbit(UInt16 CardNo, UInt16 Crd, UInt16 bitno, UInt16 on_off, double ReverseTime);
    // Xóa IO chưa thực hiện xong trong đoạn (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_clear_io_action", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_clear_io_action(UInt16 CardNo, UInt16 Crd, UInt32 IoMask);
    // Trạng thái đầu ra IO khi tạm dừng hoặc bất thường (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_pause_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_set_pause_output(UInt16 CardNo, UInt16 Crd, UInt16 action, Int32 mask, Int32 state);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_pause_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_get_pause_output(UInt16 CardNo, UInt16 Crd, ref UInt16 action, ref Int32 mask, ref Int32 state);
    // Lệnh trễ (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_delay", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_delay(UInt16 CardNo, UInt16 Crd, double delay_time, Int32 mark);
    // Đảo ngược IO trễ (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_reverse_outbit(UInt16 CardNo, UInt16 Crd, UInt16 bitno, double reverse_time);
    // Xuất IO trễ (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_delay_outbit(UInt16 CardNo, UInt16 Crd, UInt16 bitno, UInt16 on_off, double delay_time);
    // Chuyển động đơn trục trong nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_pmove_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_pmove_unit(UInt16 CardNo, UInt16 Crd, UInt16 Axis, double dist, UInt16 posi_mode, UInt16 mode, Int32 mark);
    // Nội suy đường thẳng trong nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_line_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_line_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, UInt16 posi_mode, Int32 mark);
    // Nội suy cung tròn xác định tâm trong nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_arc_move_center_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_arc_move_center_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double[] Cen_Pos, UInt16 Arc_Dir, Int32 Circle, UInt16 posi_mode, Int32 mark);
    // Nội suy cung tròn xác định bán kính trong nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_arc_move_radius_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_arc_move_radius_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double Arc_Radius, UInt16 Arc_Dir, Int32 Circle, UInt16 posi_mode, Int32 mark);
    // Nội suy cung tròn qua 3 điểm trong nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_arc_move_3points_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_arc_move_3points_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double[] Mid_Pos, Int32 Circle, UInt16 posi_mode, Int32 mark);
    // Nội suy hình chữ nhật trong nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_rectangle_move_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_rectangle_move_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] TargetPos, double[] MaskPos, Int32 Count, UInt16 rect_mode, UInt16 posi_mode, Int32 mark);
    // Thiết lập chế độ nội suy xoắn ốc (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_involute_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_set_involute_mode(UInt16 CardNo, UInt16 Crd, UInt16 mode);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_involute_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_get_involute_mode(UInt16 CardNo, UInt16 Crd, ref UInt16 mode);
    // (Dự phòng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_line_unit_extern(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double[] Cen_Pos, UInt16 posi_mode, Int32 mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_arc_move_center_unit_extern(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double[] Cen_Pos, double Arc_Radius, UInt16 posi_mode, Int32 mark);
    // Thiết lập/Đọc chế độ bám đuổi Gantry (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_gear_follow_profile(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 master_axis, double ratio);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_gear_follow_profile(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 master_axis, ref double ratio);

    //-------------------- Điều khiển PWM ----------------------
    // Điều khiển PWM (Dự phòng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_pwm_pin(UInt16 CardNo, UInt16 portno, UInt16 ON_OFF, double dfreqency, double dduty);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_pwm_pin(UInt16 CardNo, UInt16 portno, ref UInt16 ON_OFF, ref double dfreqency, ref double dduty);
    // Thiết lập/Đọc kích hoạt PWM (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_pwm_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_pwm_enable(UInt16 CardNo, UInt16 enable);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_pwm_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_pwm_enable(UInt16 CardNo, ref UInt16 enable);
    // Thiết lập/Đọc xuất PWM tức thì (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_pwm_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_pwm_output(UInt16 CardNo, UInt16 pwm_no, double fDuty, double fFre);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_pwm_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_pwm_output(UInt16 CardNo, UInt16 pwm_no, ref double fDuty, ref double fFre);
    // Xuất PWM trong nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_pwm_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_set_pwm_output(UInt16 CardNo, UInt16 Crd, UInt16 pwm_no, double fDuty, double fFre);
    // Chức năng PWM tốc độ cao (Dự phòng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_pwm_enable_extern(UInt16 CardNo, UInt16 channel, UInt16 enable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_pwm_enable_extern(UInt16 CardNo, UInt16 channel, ref UInt16 enable);
    // Thiết lập Duty cycle tương ứng khi PWM bật/tắt (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_pwm_onoff_duty", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_pwm_onoff_duty(UInt16 CardNo, UInt16 PwmNo, double fOnDuty, double fOffDuty);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_pwm_onoff_duty", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_pwm_onoff_duty(UInt16 CardNo, UInt16 PwmNo, ref double fOnDuty, ref double fOffDuty);
    // PWM bám đuổi vận tốc trong nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_pwm_follow_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_set_pwm_follow_speed(UInt16 CardNo, UInt16 Crd, UInt16 pwm_no, UInt16 mode, double MaxVel, double MaxValue, double OutValue);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_pwm_follow_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_get_pwm_follow_speed(UInt16 CardNo, UInt16 Crd, UInt16 pwm_no, ref UInt16 mode, ref double MaxVel, ref double MaxValue, ref double OutValue);
    // PWM xuất IO trễ so với điểm bắt đầu quỹ đạo (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_delay_pwm_to_start", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_delay_pwm_to_start(UInt16 CardNo, UInt16 Crd, UInt16 pwmno, UInt16 on_off, double delay_value, UInt16 delay_mode, double ReverseTime);
    // PWM xuất IO sớm so với điểm kết thúc quỹ đạo (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_ahead_pwm_to_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_ahead_pwm_to_stop(UInt16 CardNo, UInt16 Crd, UInt16 pwmno, UInt16 on_off, double ahead_value, UInt16 ahead_mode, double ReverseTime);
    // Xuất PWM lập tức trong nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_write_pwm", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_write_pwm(UInt16 CardNo, UInt16 Crd, UInt16 pwmno, UInt16 on_off, double ReverseTime);

    //-------------------- Đầu ra ADDA ----------------------
    // Đầu ra DA hộp đấu nối, thiết lập kích hoạt DA (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_da_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_da_enable(UInt16 CardNo, UInt16 enable);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_da_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_da_enable(UInt16 CardNo, ref UInt16 enable);
    // Thiết lập đầu ra DA (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_da_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_da_output(UInt16 CardNo, UInt16 channel, double Vout);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_da_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_da_output(UInt16 CardNo, UInt16 channel, ref double Vout);
    // Đầu vào AD hộp đấu nối, đọc đầu vào AD (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_ad_input(ushort CardNo, ushort channel, ref double Vout);
    // Thiết lập đầu ra DA liên tục (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_set_da_output(UInt16 CardNo, UInt16 Crd, UInt16 channel, double Vout);
    // Thiết lập kích hoạt DA liên tục (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_da_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_set_da_enable(ushort CardNo, ushort Crd, ushort enable, ushort channel, int mark);
    // DA bám đuổi bộ đếm (Dự phòng)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_encoder_da_follow_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_encoder_da_follow_enable(ushort CardNo, ushort axis, ushort enable);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_encoder_da_follow_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_encoder_da_follow_enable(ushort CardNo, ushort axis, ref ushort enable);
    // DA bám đuổi vận tốc trong nội suy liên tục (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_da_follow_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_set_da_follow_speed(ushort CardNo, ushort Crd, ushort da_no, double MaxVel, double MaxValue, double acc_offset, double dec_offset, double acc_dist, double dec_dist);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_da_follow_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_conti_get_da_follow_speed(ushort CardNo, ushort Crd, ushort da_no, ref double MaxVel, ref double MaxValue, ref double acc_offset, ref double dec_offset, ref double acc_dist, ref double dec_dist);

    // Kích hoạt giới hạn cung tròn nhỏ (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_arc_limit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_arc_limit(UInt16 CardNo, UInt16 Crd, UInt16 Enable, double MaxCenAcc, double MaxArcError);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_arc_limit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_arc_limit(UInt16 CardNo, UInt16 Crd, ref UInt16 Enable, ref double MaxCenAcc, ref double MaxArcError);
    // (Dự phòng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_IoFilter(UInt16 CardNo, UInt16 bitno, double filter);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_IoFilter(UInt16 CardNo, UInt16 bitno, ref double filter);
    // Bù bước vít (Hàm cũ, không dùng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_lsc_index_value(UInt16 CardNo, UInt16 axis, UInt16 IndexID, Int32 IndexValue);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_lsc_index_value(UInt16 CardNo, UInt16 axis, UInt16 IndexID, ref Int32 IndexValue);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_lsc_config(UInt16 CardNo, UInt16 axis, UInt16 Origin, UInt32 Interal, UInt32 NegIndex, UInt32 PosIndex, double Ratio);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_lsc_config(UInt16 CardNo, UInt16 axis, ref UInt16 Origin, ref UInt32 Interal, ref UInt32 NegIndex, ref UInt32 PosIndex, ref double Ratio);
    // Watchdog hàm cũ, không dùng
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_watchdog(UInt16 CardNo, UInt16 enable, UInt32 time);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_call_watchdog(UInt16 CardNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_read_diagnoseData(UInt16 CardNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_set_cmd_end(UInt16 CardNo, UInt16 Crd, UInt16 enable);
    // Giới hạn mềm vùng (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_zone_limit_config(UInt16 CardNo, UInt16[] axis, UInt16[] Source, Int32 x_pos_p, Int32 x_pos_n, Int32 y_pos_p, Int32 y_pos_n, UInt16 action_para);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_zone_limit_config(UInt16 CardNo, UInt16[] axis, UInt16[] Source, ref Int32 x_pos_p, ref Int32 x_pos_n, ref Int32 y_pos_p, ref Int32 y_pos_n, ref UInt16 action_para);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_zone_limit_enable(UInt16 CardNo, UInt16 enable);
    // Chức năng Interlock trục (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_interlock_config(UInt16 CardNo, UInt16[] axis, UInt16[] Source, Int32 delta_pos, UInt16 action_para);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_interlock_config(UInt16 CardNo, UInt16[] axis, UInt16[] Source, ref Int32 delta_pos, ref UInt16 action_para);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_interlock_enable(UInt16 CardNo, UInt16 enable);
    // Bảo vệ lỗi chế độ Gantry (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_grant_error_protect(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt32 dstp_error, UInt32 emg_error);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_grant_error_protect(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt32 dstp_error, ref UInt32 emg_error);
    // Bảo vệ lỗi chế độ Gantry hàm unit (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_grant_error_protect_unit(UInt16 CardNo, UInt16 axis, UInt16 enable, double dstp_error, double emg_error);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_grant_error_protect_unit(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref double dstp_error, ref double emg_error);

    // Chức năng phân loại vật phẩm (Dành riêng cho Firmware phân loại)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_camerablow_config(UInt16 CardNo, UInt16 camerablow_en, Int32 cameraPos, UInt16 piece_num, Int32 piece_distance, UInt16 axis_sel, Int32 latch_distance_min);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_camerablow_config(UInt16 CardNo, ref UInt16 camerablow_en, ref Int32 cameraPos, ref UInt16 piece_num, ref Int32 piece_distance, ref UInt16 axis_sel, ref Int32 latch_distance_min);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_clear_camerablow_errorcode(UInt16 CardNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_camerablow_errorcode(UInt16 CardNo, ref UInt16 errorcode);
    // Cấu hình đầu vào thông dụng (0~15) làm tín hiệu giới hạn trục (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_io_limit_config(UInt16 CardNo, UInt16 portno, UInt16 enable, UInt16 axis_sel, UInt16 el_mode, UInt16 el_logic);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_io_limit_config(UInt16 CardNo, UInt16 portno, ref UInt16 enable, ref UInt16 axis_sel, ref UInt16 el_mode, ref UInt16 el_logic);
    // Tham số lọc tay quay (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_handwheel_filter(UInt16 CardNo, UInt16 axis, double filter_factor);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_handwheel_filter(UInt16 CardNo, UInt16 axis, ref double filter_factor);
    // Đọc tọa độ quy hoạch hiện tại của các trục trong hệ tọa độ (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_get_interp_map(UInt16 CardNo, UInt16 Crd, ref UInt16 AxisNum, UInt16[] AxisList, double[] pPosList);
    // Mã lỗi hệ tọa độ (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_get_crd_errcode(UInt16 CardNo, UInt16 Crd, ref UInt16 errcode);
    // Dành riêng
    [DllImport("LTDMC.dll")]
    public static extern short dmc_line_unit_follow(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Dist, UInt16 posi_mode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_line_unit_follow(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] pPosList, UInt16 posi_mode, Int32 mark);
    // Thao tác DA trong vùng đệm nội suy liên tục (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_set_da_action(UInt16 CardNo, UInt16 Crd, UInt16 mode, UInt16 portno, double dvalue);
    // Đọc vận tốc bộ đếm (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_read_encoder_speed(UInt16 CardNo, UInt16 Axis, ref double current_speed);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_axis_follow_line_enable(UInt16 CardNo, UInt16 Crd, UInt16 enable_flag);
    // Bù xung trục nội suy (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_interp_compensation(UInt16 CardNo, UInt16 axis, double dvalue, double time);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_interp_compensation(UInt16 CardNo, UInt16 axis, ref double dvalue, ref double time);
    // Đọc khoảng cách so với điểm bắt đầu (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_distance_to_start(UInt16 CardNo, UInt16 Crd, ref double distance_x, ref double distance_y, Int32 imark);
    // Thiết lập cờ biểu thị có bắt đầu tính toán khoảng cách tương đối điểm bắt đầu (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_start_distance_flag(UInt16 CardNo, UInt16 Crd, UInt16 flag);

    // Bám đuổi dao cắt (Áp dụng dòng DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_gear_unit(UInt16 CardNo, UInt16 Crd, UInt16 axis, double dist, UInt16 follow_mode, Int32 imark);
    // Thiết lập kích hoạt khớp quỹ đạo (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_path_fitting_enable(UInt16 CardNo, UInt16 Crd, UInt16 enable);
    //-------------------- Bù bước vít ----------------------
    // Chức năng bù bước vít (Mới) (Áp dụng cho tất cả các thẻ Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_enable_leadscrew_comp(UInt16 CardNo, UInt16 axis, UInt16 enable);
    // Cấu hình tham số bù logic (Xung) (Áp dụng cho tất cả các thẻ Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_leadscrew_comp_config(UInt16 CardNo, UInt16 axis, UInt16 n, Int32 startpos, Int32 lenpos, Int32[] pCompPos, Int32[] pCompNeg);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_leadscrew_comp_config(UInt16 CardNo, UInt16 axis, ref UInt16 n, ref int startpos, ref int lenpos, int[] pCompPos, int[] pCompNeg);
    // Cấu hình tham số bù logic (Unit) (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_leadscrew_comp_config_unit(UInt16 CardNo, UInt16 axis, UInt16 n, double startpos, double lenpos, double[] pCompPos, double[] pCompNeg);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_leadscrew_comp_config_unit(UInt16 CardNo, UInt16 axis, ref UInt16 n, ref double startpos, ref double lenpos, double[] pCompPos, double[] pCompNeg);
    // Vị trí xung trước khi bù bước vít, vị trí bộ đếm (Áp dụng dòng DMC3000 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_position_ex(UInt16 CardNo, UInt16 axis, ref double pos);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_encoder_ex(UInt16 CardNo, UInt16 axis, ref double pos);
    // Vị trí xung trước khi bù bước vít, vị trí bộ đếm (unit) (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_position_ex_unit(UInt16 CardNo, UInt16 axis, ref double pos);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_encoder_ex_unit(UInt16 CardNo, UInt16 axis, ref double pos);

    // Chuyển động trục theo độ dài cố định, chạy theo đường cong quy định (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_t_pmove_extern(UInt16 CardNo, UInt16 axis, double MidPos, double TargetPos, double Min_Vel, double Max_Vel, double stop_Vel, double acc, double dec, UInt16 posi_mode);
    //
    [DllImport("LTDMC.dll")]
    public static extern short dmc_t_pmove_extern_unit(UInt16 CardNo, UInt16 axis, double MidPos, double TargetPos, double Min_Vel, double Max_Vel, double stop_Vel, double acc, double dec, UInt16 posi_mode);
    // Thiết lập giá trị ngưỡng cảnh báo sai số giữa bộ đếm xung và giá trị phản hồi (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LeTDMC.dll")]
    public static extern short dmc_set_pulse_encoder_count_error(UInt16 CardNo, UInt16 axis, UInt16 error);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_pulse_encoder_count_error(UInt16 CardNo, UInt16 axis, ref UInt16 error);
    // Kiểm tra xem sai số giữa bộ đếm xung và phản hồi có vượt ngưỡng (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_check_pulse_encoder_count_error(UInt16 CardNo, UInt16 axis, ref Int32 pulse_position, ref Int32 enc_position);
    // Thiết lập/Đọc ngưỡng cảnh báo sai số unit (Áp dụng dòng DMC5X10 Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_pulse_encoder_count_error_unit(ushort CardNo, ushort axis, double error);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_pulse_encoder_count_error_unit(ushort CardNo, ushort axis, ref double error);
    // Kiểm tra xem sai số unit có vượt ngưỡng (Áp dụng dòng DMC5X10 Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_check_pulse_encoder_count_error_unit(ushort CardNo, ushort axis, ref double pulse_position, ref double enc_position);
    // Kích hoạt và thiết lập chế độ dừng trục khi sai số bộ đếm theo dõi nằm ngoài phạm vi (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_encoder_count_error_action_config(UInt16 CardNo, UInt16 enable, UInt16 stopmode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_encoder_count_error_action_config(UInt16 CardNo, ref UInt16 enable, ref UInt16 stopmode);

    // Chức năng phân loại vật phẩm mới (Dành riêng cho Firmware phân loại)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_close(UInt16 CardNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_start(UInt16 CardNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_init_config(UInt16 CardNo, UInt16 cameraCount, Int32[] pCameraPos, UInt16[] pCamIONo, UInt32 cameraTime, UInt16 cameraTrigLevel, UInt16 blowCount, Int32[] pBlowPos, UInt16[] pBlowIONo, UInt32 blowTime, UInt16 blowTrigLevel, UInt16 axis, UInt16 dir, UInt16 checkMode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_camera_trig_count(UInt16 CardNo, UInt16 cameraNum, UInt32 cameraTrigCnt);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_get_camera_trig_count(UInt16 CardNo, UInt16 cameraNum, ref UInt32 pCameraTrigCnt, UInt16 count);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_blow_trig_count(UInt16 CardNo, UInt16 blowNum, UInt32 blowTrigCnt);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_get_blow_trig_count(UInt16 CardNo, UInt16 blowNum, ref UInt32 pBlowTrigCnt, UInt16 count);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_get_camera_config(UInt16 CardNo, UInt16 index, ref Int32 pos, ref UInt32 trigTime, ref UInt16 ioNo, ref UInt16 trigLevel);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_get_blow_config(UInt16 CardNo, UInt16 index, ref Int32 pos, ref UInt32 trigTime, ref UInt16 ioNo, ref UInt16 trigLevel);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_get_blow_status(UInt16 CardNo, ref Int32 trigCntAll, ref UInt16 trigMore, ref UInt16 trigLess);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_trig_blow(UInt16 CardNo, UInt16 blowNum);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_blow_enable(UInt16 CardNo, UInt16 blowNum, UInt16 enable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_piece_config(UInt16 CardNo, UInt32 maxWidth, UInt32 minWidth, UInt32 minDistance, UInt32 minTimeIntervel);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_get_piece_status(UInt16 CardNo, ref UInt32 pieceFind, ref UInt32 piecePassCam, ref UInt32 dist2next, ref UInt32 pieceWidth);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_cam_trig_phase(UInt16 CardNo, UInt16 blowNo, double coef);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_blow_trig_phase(UInt16 CardNo, UInt16 blowNo, double coef);

    // Dùng nội bộ (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_sevon_enable(UInt16 CardNo, UInt16 axis, UInt16 on_off);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_sevon_enable(UInt16 CardNo, UInt16 axis);

    // DA bám đuổi bộ đếm liên tục (Áp dụng dòng DMC5000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_set_encoder_da_follow_enable(UInt16 CardNo, UInt16 Crd, UInt16 axis, UInt16 enable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_get_encoder_da_follow_enable(UInt16 CardNo, UInt16 Crd, ref UInt16 axis, ref UInt16 enable);
    // Thiết lập dải sai số vị trí (Áp dụng cho tất cả thẻ Pulse, thẻ Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_set_factor_error", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_set_factor_error(UInt16 CardNo, UInt16 axis, double factor, Int32 error);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_get_factor_error", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_get_factor_error(UInt16 CardNo, UInt16 axis, ref double factor, ref Int32 error);
    // Thiết lập dải sai số vị trí unit (Áp dụng thẻ DMC5X10 Pulse, thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_factor_error_unit(ushort CardNo, ushort axis, double factor, double error);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_factor_error_unit(ushort CardNo, ushort axis, ref double factor, ref double error);
    // Dự phòng
    [DllImport("LTDMC.dll")]
    public static extern short dmc_check_done_pos(UInt16 CardNo, UInt16 axis, UInt16 posi_mode);
    // Dự phòng
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_factor(UInt16 CardNo, UInt16 axis, double factor);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_error(UInt16 CardNo, UInt16 axis, Int32 error);
    // Kiểm tra lệnh đã đến vị trí (Áp dụng cho tất cả thẻ Pulse, thẻ Bus)
    [DllImport("LTDMC.dll", EntryPoint = "dmc_check_success_pulse", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_check_success_pulse(UInt16 CardNo, UInt16 axis);
    [DllImport("LTDMC.dll", EntryPoint = "dmc_check_success_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short dmc_check_success_encoder(UInt16 CardNo, UInt16 axis);

    // Chức năng đếm bộ đếm và IO (Dành riêng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_io_count_profile(UInt16 CardNo, UInt16 chan, UInt16 bitno, UInt16 mode, double filter, double count_value, UInt16[] axis_list, UInt16 axis_num, UInt16 stop_mode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_io_count_profile(UInt16 CardNo, UInt16 chan, ref UInt16 bitno, ref UInt16 mode, ref double filter, ref double count_value, UInt16[] axis_list, ref UInt16 axis_num, ref UInt16 stop_mode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_io_count_enable(UInt16 CardNo, UInt16 chan, UInt16 ifenable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_clear_io_count(UInt16 CardNo, UInt16 chan);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_io_count_value_extern(UInt16 CardNo, UInt16 chan, ref Int32 current_value);
    // Dự phòng
    [DllImport("LTDMC.dll")]
    public static extern short dmc_change_speed_extend(UInt16 CardNo, UInt16 axis, double Curr_Vel, double Taccdec, UInt16 pin_num, UInt16 trig_mode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_follow_vector_speed_move(UInt16 CardNo, UInt16 axis, UInt16 Follow_AxisNum, UInt16[] Follow_AxisList, double ratio);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_line_unit_extend(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] pPosList, UInt16 posi_mode, double Extend_Len, UInt16 enable, Int32 mark);

    // Tham số thẻ Bus
    [DllImport("LTDMC.dll", EntryPoint = "nmc_download_configfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short nmc_download_configfile(UInt16 CardNo, UInt16 PortNum, String FileName); // Tệp cấu hình ENI Bus
    [DllImport("LTDMC.dll", EntryPoint = "nmc_download_mapfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short nmc_download_mapfile(UInt16 CardNo, String FileName);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_upload_configfile(UInt16 CardNo, UInt16 PortNum, String FileName);
    [DllImport("LTDMC.dll", EntryPoint = "nmc_set_manager_para", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short nmc_set_manager_para(UInt16 CardNo, UInt16 PortNum, Int32 baudrate, UInt16 ManagerID);
    [DllImport("LTDMC.dll", EntryPoint = "nmc_get_manager_para", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short nmc_get_manager_para(UInt16 CardNo, UInt16 PortNum, ref UInt32 baudrate, ref UInt16 ManagerID);
    [DllImport("LTDMC.dll", EntryPoint = "nmc_set_manager_od", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short nmc_set_manager_od(UInt16 CardNo, UInt16 PortNum, UInt16 index, UInt16 subindex, UInt16 valuelength, UInt32 value);
    [DllImport("LTDMC.dll", EntryPoint = "nmc_get_manager_od", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short nmc_get_manager_od(UInt16 CardNo, UInt16 PortNum, UInt16 index, UInt16 subindex, UInt16 valuelength, ref UInt32 value);

    [DllImport("LTDMC.dll", EntryPoint = "nmc_get_total_axes", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short nmc_get_total_axes(ushort CardNo, ref uint TotalAxis);
    [DllImport("LTDMC.dll", EntryPoint = "nmc_get_total_ionum", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short nmc_get_total_ionum(UInt16 CardNo, ref UInt16 TotalIn, ref UInt16 TotalOut);
    [DllImport("LTDMC.dll", EntryPoint = "nmc_get_LostHeartbeat_Nodes", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short nmc_get_LostHeartbeat_Nodes(UInt16 CardNo, UInt16 PortNum, UInt16[] NodeID, ref UInt16 NodeNum);
    [DllImport("LTDMC.dll", EntryPoint = "nmc_get_EmergeneyMessege_Nodes", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short nmc_get_EmergeneyMessege_Nodes(UInt16 CardNo, UInt16 PortNum, UInt32[] NodeMsg, ref UInt16 MsgNum);
    [DllImport("LTDMC.dll", EntryPoint = "nmc_SendNmtCommand", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short nmc_SendNmtCommand(UInt16 CardNo, UInt16 PortNum, UInt16 NodeID, UInt16 NmtCommand);
    [DllImport("LTDMC.dll", EntryPoint = "nmc_syn_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern short nmc_syn_move(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, Int32[] Position, UInt16[] PosiMode);
    //
    [DllImport("LTDMC.dll")]
    public static extern short nmc_syn_move_unit(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, double[] Position, UInt16[] PosiMode);
    // Chuyển động đồng bộ đa trục thẻ Bus
    [DllImport("LTDMC.dll")]
    public static extern short nmc_sync_pmove_unit(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, double[] Dist, UInt16[] PosiMode);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_sync_vmove_unit(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, UInt16[] Dir);
    // Thiết lập tham số Master
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_master_para(UInt16 CardNo, UInt16 PortNum, UInt16 Baudrate, UInt32 NodeCnt, UInt16 MasterId);
    // Đọc tham số Master
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_master_para(UInt16 CardNo, UInt16 PortNum, ref UInt16 Baudrate, ref UInt32 NodeCnt, ref UInt16 MasterId);
    // Lấy số lượng cổng ADDA của tổng tuyến
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_total_adcnum(ushort CardNo, ref ushort TotalIn, ref ushort TotalOut);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_controller_workmode(ushort CardNo, ushort controller_mode);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_controller_workmode(ushort CardNo, ref ushort controller_mode);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_cycletime(ushort CardNo, ushort FieldbusType, int CycleTime);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_cycletime(ushort CardNo, ushort FieldbusType, ref int CycleTime);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_node_od(ushort CardNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, ref int value);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_node_od(ushort CardNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, int value);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_reset_to_factory(ushort CardNo, ushort PortNum, ushort NodeNum);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_alarm_clear(ushort CardNo, ushort PortNum, ushort nodenum);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_slave_nodes(ushort CardNo, ushort PortNum, ushort BaudRate, ref ushort NodeId, ref ushort NodeNum);

    // Máy trạng thái trục
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_axis_state_machine(ushort CardNo, ushort axis, ref ushort Axis_StateMachine);
    // Lấy từ trạng thái trục
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_axis_statusword(ushort CardNo, ushort axis, ref int statusword);
    // Lấy chế độ điều khiển cấu hình trục (6-về gốc, 8-csp)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_axis_setting_contrlmode(ushort CardNo, ushort axis, ref int contrlmode);
    // Thiết lập từ điều khiển trục thẻ Bus
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_axis_contrlword(ushort CardNo, ushort axis, int contrlword);
    // Lấy từ điều khiển trục thẻ Bus
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_axis_contrlword(ushort CardNo, ushort axis, ref int contrlword);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_axis_type(ushort CardNo, ushort axis, ref ushort Axis_Type);
    // Lấy thời gian tiêu thụ tổng tuyến: TB, Max, số chu kỳ
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_consume_time_fieldbus(ushort CardNo, ushort Fieldbustype, ref uint Average_time, ref uint Max_time, ref UInt64 Cycles);
    // Xóa dữ liệu thời gian
    [DllImport("LTDMC.dll")]
    public static extern short nmc_clear_consume_time_fieldbus(ushort CardNo, ushort Fieldbustype);
    // Kích hoạt/Vô hiệu hóa trục đơn thẻ Bus (255 đại diện tất cả)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_axis_enable(ushort CardNo, ushort axis);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_axis_disable(ushort CardNo, ushort axis);
    // Lấy thông tin địa chỉ Slave của trục
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_axis_node_address(ushort CardNo, ushort axis, ref ushort SlaveAddr, ref ushort Sub_SlaveAddr);
    // Lấy số lượng Slave tổng tuyến
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_total_slaves(ushort CardNo, ushort PortNum, ref ushort TotalSlaves);
    [DllImport("LTDMC.dll")]
    // Hàm về gốc tổng tuyến
    public static extern short nmc_set_home_profile(ushort CardNo, ushort axis, ushort home_mode, double Low_Vel, double High_Vel, double Tacc, double Tdec, double offsetpos);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_home_profile(ushort CardNo, ushort axis, ref ushort home_mode, ref double Low_Vel, ref double High_Vel, ref double Tacc, ref double Tdec, ref double offsetpos);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_home_move(ushort CardNo, ushort axis);
    //
    [DllImport("LTDMC.dll")]
    public static extern short nmc_start_scan_ethercat(ushort CardNo, ushort AddressID);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_stop_scan_ethercat(ushort CardNo, ushort AddressID);
    // Thiết lập chế độ chạy trục (1 là pp, 6 là về gốc, 8 là csp)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_axis_run_mode(ushort CardNo, ushort axis, ushort run_mode);
    // Xóa cảnh báo cổng
    [DllImport("LTDMC.dll")]
    public static extern short nmc_clear_alarm_fieldbus(ushort CardNo, ushort PortNum);
    // Dừng tổng tuyến ethercat (0-thành công)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_stop_etc(ushort CardNo, ref ushort ETCState);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_axis_contrlmode(ushort CardNo, ushort Axis, ref int Contrlmode);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_axis_io_in(ushort CardNo, ushort axis);

    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_axis_io_out(UInt16 CardNo, UInt16 axis, UInt32 iostate);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_axis_io_out(UInt16 CardNo, UInt16 axis);
    // Lấy mã lỗi cổng tổng tuyến
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_errcode(ushort CardNo, ushort channel, ref ushort errcode);
    // Xóa mã lỗi cổng tổng tuyến
    [DllImport("LTDMC.dll")]
    public static extern short nmc_clear_errcode(ushort CardNo, ushort channel);
    // Lấy mã lỗi trục tổng tuyến
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_axis_errcode(ushort CardNo, ushort axis, ref ushort Errcode);
    // Xóa mã lỗi trục tổng tuyến
    [DllImport("LTDMC.dll")]
    public static extern short nmc_clear_axis_errcode(ushort CardNo, ushort axis);

    // Hàm mở rộng thư viện RTEX
    [DllImport("LTDMC.dll")]
    public static extern short nmc_start_connect(UInt16 CardNo, UInt16 chan, ref UInt16 info, ref UInt16 len);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_vendor_info(UInt16 CardNo, UInt16 axis, Byte[] info, ref UInt16 len);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_slave_type_info(UInt16 CardNo, UInt16 axis, Byte[] info, ref UInt16 len);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_slave_name_info(UInt16 CardNo, UInt16 axis, Byte[] info, ref UInt16 len);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_slave_version_info(UInt16 CardNo, UInt16 axis, Byte[] info, ref UInt16 len);

    [DllImport("LTDMC.dll")]
    public static extern short nmc_write_parameter(UInt16 CardNo, UInt16 axis, UInt16 index, UInt16 subindex, UInt32 para_data);
    // Ghi vào EEPROM driver RTEX
    [DllImport("LTDMC.dll")]
    public static extern short nmc_write_slave_eeprom(UInt16 CardNo, UInt16 axis);

    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_parameter(UInt16 CardNo, UInt16 axis, UInt16 index, UInt16 subindex, ref UInt32 para_data);
    // Đọc thuộc tính tham số (RTEX)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_parameter_attributes(UInt16 CardNo, UInt16 axis, UInt16 index, UInt16 subindex, ref UInt32 para_data);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_cmdcycletime(UInt16 CardNo, UInt16 PortNum, UInt32 cmdtime);
    // Thiết lập tỷ lệ chu kỳ lệnh RTEX (us)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_cmdcycletime(UInt16 CardNo, UInt16 PortNum, ref UInt32 cmdtime);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_config_atuo_log(UInt16 CardNo, UInt16 ifenable, UInt16 dir, UInt16 byte_index, UInt16 mask, UInt16 condition, UInt32 counter);

    // Mở rộng PDO
    [DllImport("LTDMC.dll")]
    public static extern short nmc_write_rxpdo_extra(UInt16 CardNo, UInt16 PortNum, UInt16 address, UInt16 DataLen, Int32 Value);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_rxpdo_extra(UInt16 CardNo, UInt16 PortNum, UInt16 address, UInt16 DataLen, ref Int32 Value);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_txpdo_extra(UInt16 CardNo, UInt16 PortNum, UInt16 address, UInt16 DataLen, ref Int32 Value);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_write_rxpdo_extra_uint(UInt16 CardNo, UInt16 PortNum, UInt16 address, UInt16 DataLen, UInt32 Value);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_rxpdo_extra_uint(UInt16 CardNo, UInt16 PortNum, UInt16 address, UInt16 DataLen, ref UInt32 Value);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_txpdo_extra_uint(UInt16 CardNo, UInt16 PortNum, UInt16 address, UInt16 DataLen, ref UInt32 Value);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_log_state(UInt16 CardNo, UInt16 chan, ref UInt32 state);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_driver_reset(UInt16 CardNo, UInt16 axis);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_offset_pos(UInt16 CardNo, UInt16 axis, double offset_pos);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_offset_pos(UInt16 CardNo, UInt16 axis, ref double offset_pos);
    // Xóa giá trị nhiều vòng của encoder tuyệt đối rtex
    [DllImport("LTDMC.dll")]
    public static extern short nmc_clear_abs_driver_multi_cycle(UInt16 CardNo, UInt16 axis);
    //--------------------------- Thao tác mô-đun mở rộng EtherCAT IO ----------------------
    // Thiết lập đầu ra Port 32 bit mở rộng tổng tuyến
    [DllImport("LTDMC.dll")]
    public static extern short nmc_write_outport_extern(UInt16 CardNo, UInt16 Channel, UInt16 NoteID, UInt16 portno, UInt32 outport_val);
    // Đọc đầu ra Port 32 bit mở rộng tổng tuyến
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_outport_extern(UInt16 CardNo, UInt16 Channel, UInt16 NoteID, UInt16 portno, ref UInt32 outport_val);
    // Đọc đầu vào Port 32 bit mở rộng tổng tuyến
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_inport_extern(UInt16 CardNo, UInt16 Channel, UInt16 NoteID, UInt16 portno, ref UInt32 inport_val);
    // Thiết lập đầu ra IO
    [DllImport("LTDMC.dll")]
    public static extern short nmc_write_outbit_extern(UInt16 CardNo, UInt16 Channel, UInt16 NoteID, UInt16 IoBit, UInt16 IoValue);
    // Đọc đầu ra IO
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_outbit_extern(UInt16 CardNo, UInt16 Channel, UInt16 NoteID, UInt16 IoBit, ref UInt16 IoValue);
    // Đọc đầu vào IO
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_inbit_extern(UInt16 CardNo, UInt16 Channel, UInt16 NoteID, UInt16 IoBit, ref UInt16 IoValue);

    // Trả về mã lỗi gần nhất
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_current_fieldbus_state_info(UInt16 CardNo, UInt16 Channel, ref UInt16 Axis, ref UInt16 ErrorType, ref ushort SlaveAddr, ref UInt32 ErrorFieldbusCode);
    // Trả về lịch sử mã lỗi
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_detail_fieldbus_state_info(UInt16 CardNo, UInt16 Channel, UInt32 ReadErrorNum, ref UInt32 TotalNum, ref UInt32 ActualNum, UInt16[] Axis, UInt16[] ErrorType, UInt16[] SlaveAddr, UInt32[] ErrorFieldbusCode);
    // Bắt đầu thu thập (PDO Trace)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_start_pdo_trace(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, UInt16 Index_Num, UInt32 Trace_Len, UInt16[] Index, UInt16[] Sub_Index);
    // Lấy tham số thu thập
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_pdo_trace(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, ref UInt16 Index_Num, ref UInt32 Trace_Len, UInt16[] Index, UInt16[] Sub_Index);
    // Thiết lập tham số thu thập kích hoạt
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_pdo_trace_trig_para(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, UInt16 Trig_Index, UInt16 Trig_Sub_Index, int Trig_Value, UInt16 Trig_Mode);
    // Lấy tham số thu thập kích hoạt
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_pdo_trace_trig_para(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, ref UInt16 Trig_Index, ref UInt16 Trig_Sub_Index, ref int Trig_Value, ref UInt16 Trig_Mode);
    // Xóa dữ liệu thu thập
    [DllImport("LTDMC.dll")]
    public static extern short nmc_clear_pdo_trace_data(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr);
    // Dừng thu thập
    [DllImport("LTDMC.dll")]
    public static extern short nmc_stop_pdo_trace(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr);
    // Đọc dữ liệu thu thập
    [DllImport("LTDMC.dll")]
    public static extern short nmc_read_pdo_trace_data(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, UInt32 StartAddr, UInt32 Readlen, ref UInt32 ActReadlen, Byte[] Data);
    // Số lượng dữ liệu đã thu thập
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_pdo_trace_num(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, ref UInt32 Data_num, ref UInt32 Size_of_each_bag);
    // Trạng thái thu thập
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_pdo_trace_state(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, ref UInt16 Trace_state);
    // Dành riêng cho tổng tuyến
    [DllImport("LTDMC.dll")]
    public static extern short nmc_reset_canopen(UInt16 CardNo);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_reset_rtex(UInt16 CardNo);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_reset_etc(UInt16 CardNo);
    // Cấu hình xử lý lỗi tổng tuyến
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_fieldbus_error_switch(UInt16 CardNo, UInt16 channel, UInt16 data);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_fieldbus_error_switch(UInt16 CardNo, UInt16 channel, ref UInt16 data);

    // Chuyển động Torque
    [DllImport("LTDMC.dll")]
    public static extern short nmc_torque_move(UInt16 CardNo, UInt16 axis, int Torque, UInt16 PosLimitValid, double PosLimitValue, UInt16 PosMode);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_change_torque(UInt16 CardNo, UInt16 axis, int Torque);
    // Đọc độ lớn Torque
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_torque(UInt16 CardNo, UInt16 axis, ref int Torque);
    // Hàm Modbus
    [DllImport("LTDMC.dll")]
    public static extern short dmc_modbus_active_COM1(UInt16 id, string COMID, int speed, int bits, int check, int stop);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_modbus_active_COM2(UInt16 id, string COMID, int speed, int bits, int check, int stop);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_modbus_active_ETH(UInt16 id, UInt16 port);

    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_modbus_0x(UInt16 CardNo, UInt16 start, UInt16 inum, byte[] pdata);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_modbus_0x(UInt16 CardNo, UInt16 start, UInt16 inum, byte[] pdata);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_modbus_4x(UInt16 CardNo, UInt16 start, UInt16 inum, UInt16[] pdata);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_modbus_4x(UInt16 CardNo, UInt16 start, UInt16 inum, UInt16[] pdata);

    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_modbus_4x_float(UInt16 CardNo, UInt16 start, UInt16 inum, float[] pdata);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_modbus_4x_float(UInt16 CardNo, UInt16 start, UInt16 inum, float[] pdata);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_modbus_4x_int(UInt16 CardNo, UInt16 start, UInt16 inum, int[] pdata);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_modbus_4x_int(UInt16 CardNo, UInt16 start, UInt16 inum, int[] pdata);
    // Dự phòng
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_line_io_union(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] pPosList, UInt16 posi_mode, UInt16 bitno, UInt16 on_off, double io_value, UInt16 io_mode, UInt16 MapAxis, UInt16 pos_source, double ReverseTime, long mark);
    // Thiết lập hướng bộ đếm (Áp dụng dòng DMC3000 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_encoder_dir(UInt16 CardNo, UInt16 axis, UInt16 dir);

    // Giới hạn mềm vùng cung tròn (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_arc_zone_limit_config(UInt16 CardNo, UInt16[] AxisList, UInt16 AxisNum, double[] Center, double Radius, UInt16 Source, UInt16 StopMode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_arc_zone_limit_config(UInt16 CardNo, UInt16[] AxisList, ref UInt16 AxisNum, double[] Center, ref double Radius, ref UInt16 Source, ref UInt16 StopMode);
    // Giới hạn mềm vùng cung tròn unit (Áp dụng dòng DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_arc_zone_limit_config_unit(ushort CardNo, ushort[] AxisList, ushort AxisNum, double[] Center, double Radius, ushort Source, ushort StopMode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_arc_zone_limit_config_unit(ushort CardNo, ushort[] AxisList, ref ushort AxisNum, double[] Center, ref double Radius, ref ushort Source, ref ushort StopMode);
    // Truy vấn trạng thái các trục tương ứng (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_arc_zone_limit_axis_status(UInt16 CardNo, UInt16 axis);
    // Kích hoạt giới hạn vùng cung tròn (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_arc_zone_limit_enable(UInt16 CardNo, UInt16 enable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_arc_zone_limit_enable(UInt16 CardNo, ref UInt16 enable);

    // Có khởi tạo mức điện đầu ra sau khi hộp đấu nối mất điện hay không
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_output_status_repower(UInt16 CardNo, UInt16 enable);
    // Hàm cũ, không dùng
    [DllImport("LTDMC.dll")]
    public static extern short dmc_t_pmove_extern_softlanding(UInt16 CardNo, UInt16 axis, double MidPos, double TargetPos, double start_Vel, double Max_Vel, double stop_Vel, UInt32 delay_ms, double Max_Vel2, double stop_vel2, double acc_time, double dec_time, UInt16 posi_mode);
    // Dự phòng
    [DllImport("LTDMC.dll")]
    public static extern short dmc_compare_add_point_XD(UInt16 CardNo, UInt16 cmp, long pos, UInt16 dir, UInt16 action, UInt32 actpara, long startPos);

    //--------------------------- Cấu hình chuyển vận tốc/vị trí Online khi kích hoạt đầu vào ORG ----------------------
    // Cấu hình chuyển vận tốc/vị trí Online khi kích hoạt ORG (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pmove_change_pos_speed_config(UInt16 CardNo, UInt16 axis, double tar_vel, double tar_rel_pos, UInt16 trig_mode, UInt16 source);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_pmove_change_pos_speed_config(UInt16 CardNo, UInt16 axis, ref double tar_vel, ref double tar_rel_pos, ref UInt16 trig_mode, ref UInt16 source);
    // Kích hoạt (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pmove_change_pos_speed_enable(UInt16 CardNo, UInt16 axis, UInt16 enable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_pmove_change_pos_speed_enable(UInt16 CardNo, UInt16 axis, ref UInt16 enable);
    // Đọc trạng thái: trig_num số lần kích hoạt, trig_pos vị trí kích hoạt (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_pmove_change_pos_speed_state(ushort CardNo, ushort axis, ref ushort trig_num, double[] trig_pos);
    // Thay đổi vận tốc/vị trí qua IO, cấu hình cổng IO (Áp dụng thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pmove_change_pos_speed_inbit(ushort CardNo, ushort axis, ushort inbit, ushort enable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_pmove_change_pos_speed_inbit(ushort CardNo, ushort axis, ref ushort inbit, ref ushort enable);
    // Dự phòng
    [DllImport("LTDMC.dll")]
    public static extern short dmc_compare_add_point_extend(UInt16 CardNo, UInt16 axis, long pos, UInt16 dir, UInt16 action, UInt16 para_num, ref UInt32 actpara, UInt32 compare_time);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_cmd_position(UInt16 CardNo, UInt16 axis, ref double pos);
    // Cấu hình lấy mẫu phân tích logic (Dùng nội bộ)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_logic_analyzer_config(UInt16 CardNo, UInt16 channel, UInt32 SampleFre, UInt32 SampleDepth, UInt16 SampleMode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_start_logic_analyzer(UInt16 CardNo, UInt16 channel, UInt16 enable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_logic_analyzer_counter(UInt16 CardNo, UInt16 channel, ref UInt32 counter);
    // 20190923 Sửa hàm tùy chỉnh kg (Theo yêu cầu khách hàng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_read_inbit_append(UInt16 CardNo, UInt16 bitno);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_write_outbit_append(UInt16 CardNo, UInt16 bitno, UInt16 on_off);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_read_outbit_append(UInt16 CardNo, UInt16 bitno);
    [DllImport("LTDMC.dll")]
    public static extern UInt32 dmc_read_inport_append(UInt16 CardNo, UInt16 portno);
    [DllImport("LTDMC.dll")]
    public static extern UInt32 dmc_read_outport_append(UInt16 CardNo, UInt16 portno);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_write_outport_append(UInt16 CardNo, UInt16 portno, UInt32 port_value);

    //--------------------------- Nội suy Ellipse và bám đuổi Tangent ----------------------
    // Thiết lập bám đuổi Tangent hệ tọa độ (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_tangent_follow(UInt16 CardNo, UInt16 Crd, UInt16 axis, UInt16 follow_curve, UInt16 rotate_dir, double degree_equivalent);
    // Lấy tham số bám đuổi Tangent (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_tangent_follow_param(UInt16 CardNo, UInt16 Crd, ref UInt16 axis, ref UInt16 follow_curve, ref UInt16 rotate_dir, ref double degree_equivalent);
    // Hủy bám đuổi (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_disable_follow_move(UInt16 CardNo, UInt16 Crd);
    // Nội suy Ellipse (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_ellipse_move(UInt16 CardNo, UInt16 Crd, UInt16 axisNum, UInt16[] Axis_List, double[] Target_Pos, double[] Cen_Pos, double A_Axis_Len, double B_Axis_Len, UInt16 Dir, UInt16 Pos_Mode);

    //--------------------------- Chức năng Watchdog ----------------------
    // Thiết lập sự kiện phản ứng khi Watchdog kích hoạt (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_watchdog_action_event(UInt16 CardNo, UInt16 event_mask);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_watchdog_action_event(UInt16 CardNo, ref UInt16 event_mask);
    // Kích hoạt cơ chế bảo vệ Watchdog (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_watchdog_enable(UInt16 CardNo, double timer_period, UInt16 enable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_watchdog_enable(UInt16 CardNo, ref double timer_period, ref UInt16 enable);
    // Reset bộ định thời Watchdog (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_reset_watchdog_timer(UInt16 CardNo);

    // Chức năng kiểm tra IO tùy chỉnh
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_io_check_control(UInt16 CardNo, UInt16 sensor_in_no, UInt16 check_mode, UInt16 A_out_no, UInt16 B_out_no, UInt16 C_out_no, UInt16 output_mode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_io_check_control(UInt16 CardNo, ref UInt16 sensor_in_no, ref UInt16 check_mode, ref UInt16 A_out_no, ref UInt16 B_out_no, ref UInt16 C_out_no, ref UInt16 output_mode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_stop_io_check_control(UInt16 CardNo);

    // Thiết lập khoảng cách lệch khi quay lại EL (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_el_ret_deviation(UInt16 CardNo, UInt16 axis, UInt16 enable, double deviation);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_el_ret_deviation(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref double deviation);

    // Cộng dồn vị trí hai trục, chức năng so sánh tốc độ cao (Thử nghiệm)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_set_config_overlap(UInt16 CardNo, UInt16 hcmp, UInt16 axis, UInt16 cmp_source, UInt16 cmp_logic, Int32 time, UInt16 axis_num, UInt16 aux_axis, UInt16 aux_source);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_hcmp_get_config_overlap(UInt16 CardNo, UInt16 hcmp, ref UInt16 axis, ref UInt16 cmp_source, ref UInt16 cmp_logic, ref Int32 time, ref UInt16 axis_num, ref UInt16 aux_axis, ref UInt16 aux_source);

    // Nội suy xoắn ốc (Thử nghiệm, DMC5000/5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_helix_move_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AixsList, double[] StartPos, double[] TargetPos, UInt16 Arc_Dir, int Circle, UInt16 mode, int mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_helix_move_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] StartPos, double[] TargetPos, UInt16 Arc_Dir, int Circle, UInt16 mode);

    // Vùng đệm PDO 20190715 (Dùng nội bộ)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pdo_buffer_enter(UInt16 CardNo, UInt16 axis);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pdo_buffer_stop(UInt16 CardNo, UInt16 axis);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pdo_buffer_clear(UInt16 CardNo, UInt16 axis);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pdo_buffer_run_state(UInt16 CardNo, UInt16 axis, ref int RunState, ref int Remain, ref int NotRunned, ref int Runned);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pdo_buffer_add_data(UInt16 CardNo, UInt16 axis, int size, int[] data_table);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pdo_buffer_start_multi(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, UInt16[] ResultList);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pdo_buffer_pause_multi(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, UInt16[] ResultList);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_pdo_buffer_stop_multi(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, UInt16[] ResultList);
    // Dự phòng
    [DllImport("LTDMC.dll")]
    public static extern short dmc_calculate_arccenter_3point(double[] start_pos, double[] mid_pos, double[] target_pos, double[] cen_pos);

    //--------------------- Chuyển động Gantry vùng đệm lệnh ------------------
    // Chuyển động Gantry vùng đệm lệnh (Áp dụng dòng DMC3000/5000 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_set_muti_profile_unit(ushort card, ushort group, ushort axis_num, ushort[] axis_list, double[] start_vel, double[] max_vel, double[] tacc, double[] tdec, double[] stop_vel);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_set_profile_unit(ushort card, ushort group, ushort axis, double start_vel, double max_vel, double tacc, double tdec, double stop_vel);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_sigaxis_moveseg_data(ushort card, ushort group, ushort axis, double Target_pos, ushort process_mode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_sigaxis_move_twoseg_data(ushort card, ushort group, ushort axis, double Target_pos, double second_pos, double second_vel, double second_endvel, ushort process_mode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_mutiaxis_moveseg_data(ushort card, ushort group, ushort axisnum, ushort[] axis_list, double[] Target_pos, ushort process_mode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_mutiaxis_move_twoseg_data(ushort card, ushort group, ushort axisnum, ushort[] axis_list, double[] Target_pos, double[] second_pos, double[] second_vel, double[] second_endvel, ushort process_mode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_ioTrig_movseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, ushort process_mode, ushort trigINbit, ushort trigINstate, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_mutiposTrig_movseg_data(ushort card, ushort group, ushort axis, double Target_pos, ushort process_mode, ushort trigaxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_mutiposTrig_mov_twoseg_data(ushort card, ushort group, ushort axis, double Target_pos, double softland_pos, double softland_vel, double softland_endvel, ushort process_mode, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_upseg_data(ushort card, ushort group, ushort axis, double Target_pos, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_up_twoseg_data(ushort card, ushort group, ushort axis, double Target_pos, double second_pos, double second_vel, double second_endvel, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_ioPosTrig_movseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, ushort process_mode, ushort trigAxis, double trigPos, ushort trigPosType, ushort trigMode, ushort TrigINNum, ushort[] trigINList, ushort[] trigINstate, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_ioPosTrig_mov_twoseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, double[] second_pos, double[] second_vel, double[] second_endvel, ushort process_mode, ushort trigAxis, double trigPos, ushort trigPosType, ushort trigMode, ushort TrigINNum, ushort[] trigINList, ushort[] trigINstate, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_posTrig_movseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, ushort process_mode, ushort trigAxis, double trigPos, ushort trigPosType, ushort trigMode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_posTrig_mov_twoseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, double[] second_pos, double[] second_vel, double[] second_endvel, ushort process_mode, ushort trigAxis, double trigPos, ushort trigPosType, ushort trigMode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_ioPosTrig_down_seg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, ushort trigIN, ushort trigINstate, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_ioPosTrig_down_twoseg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, double second_pos, double second_vel, double second_endvel, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, ushort trigIN, ushort trigINstate, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_posTrig_down_seg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_posTrig_down_twoseg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, double second_pos, double second_vel, double second_endvel, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_posTrig_down_seg_cmd_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, ushort trigAxisNum, ushort[] trigAxisList, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_posTrig_down_twoseg_cmd_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, double second_pos, double second_vel, double second_endvel, ushort trigAxisNum, ushort[] trigAxisList, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_mutiposTrig_singledown_seg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, ushort process_mode, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_add_mutiposTrig_mutidown_seg_data(ushort card, ushort group, ushort axisnum, ushort[] axis_list, double[] safePos, double[] Target_pos, ushort process_mode, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_posTrig_outbit(ushort card, ushort group, ushort bitno, ushort on_off, ushort ahead_axis, double ahead_value, ushort ahead_PosType, ushort ahead_Mode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_mutiposTrig_outbit(ushort card, ushort group, ushort bitno, ushort on_off, ushort process_mode, ushort trigaxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_immediate_write_outbit(ushort card, ushort group, ushort bitno, ushort on_off, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_wait_input(ushort card, ushort group, ushort bitno, ushort on_off, double time_out, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_delay_time(ushort card, ushort group, double delay_time, uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_get_run_state(ushort card, ushort group, ref ushort state, ref ushort enable, ref uint stop_reason, ref ushort trig_phase, ref uint mark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_open_list(ushort card, ushort group, ushort axis_num, ushort[] axis_list);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_close_list(ushort card, ushort group);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_start_list(ushort card, ushort group);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_stop_list(ushort card, ushort group, ushort stopMode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_pause_list(ushort card, ushort group, ushort stopMode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_set_encoder_error_allow(ushort card, ushort group, double allow_error);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_m_get_encoder_error_allow(ushort card, ushort group, ref double allow_error);

    // Đọc tất cả đầu vào AD (Áp dụng dòng DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_ad_input_all(ushort CardNo, ref double Vout);
    // Sau khi tạm dừng nội suy liên tục thì dùng pmove (Áp dụng dòng DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_pmove_unit_pausemode(ushort CardNo, ushort axis, double TargetPos, double Min_Vel, double Max_Vel, double stop_Vel, double acc, double dec, double smooth_time, ushort posi_mode);
    // Sau khi dùng pmove ở chế độ tạm dừng, quay lại vị trí tạm dừng (Áp dụng dòng DMC5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_return_pausemode(ushort CardNo, ushort Crd, ushort axis);
    // Kiểm tra hộp đấu nối có hỗ trợ kiểm tra truyền thông hay không (Áp dụng dòng DMC3000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_check_if_crc_support(ushort CardNo);

    // Chức năng kiểm tra va chạm trục (Áp dụng dòng DMC3000 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_axis_conflict_config(ushort CardNo, ushort[] axis_list, ushort[] axis_depart_dir, double home_dist, double conflict_dist, ushort stop_mode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_axis_conflict_config(ushort CardNo, ushort[] axis_list, ushort[] axis_depart_dir, ref double home_dist, ref double conflict_dist, ref ushort stop_mode);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_axis_conflict_config_en(ushort CardNo, ushort enable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_axis_conflict_config_en(ushort CardNo, ref ushort enable);

    // Phân loại vật phẩm thêm kênh (Dành riêng cho Firmware phân loại)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_close_ex(ushort CardNo, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_start_ex(ushort CardNo, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_init_config_ex(ushort CardNo, ushort cameraCount, int[] pCameraPos, ushort[] pCamIONo, UInt32 cameraTime, ushort cameraTrigLevel, ushort blowCount, int[] pBlowPos, ushort[] pBlowIONo, UInt32 blowTime, ushort blowTrigLevel, ushort axis, ushort dir, ushort checkMode, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_camera_trig_count_ex(ushort CardNo, ushort cameraNum, UInt32 cameraTrigCnt, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_get_camera_trig_count_ex(ushort CardNo, ushort cameraNum, ref UInt32 pCameraTrigCnt, ushort count, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_blow_trig_count_ex(ushort CardNo, ushort blowNum, UInt32 blowTrigCnt, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_get_blow_trig_count_ex(ushort CardNo, ushort blowNum, ref UInt32 pBlowTrigCnt, ushort count, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_get_camera_config_ex(ushort CardNo, ushort index, ref int pos, ref UInt32 trigTime, ref ushort ioNo, ref ushort trigLevel, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_get_blow_config_ex(ushort CardNo, ushort index, ref int pos, ref UInt32 trigTime, ref ushort ioNo, ref ushort trigLevel, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_get_blow_status_ex(ushort CardNo, ref UInt32 trigCntAll, ref ushort trigMore, ref ushort trigLess, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_trig_blow_ex(ushort CardNo, ushort blowNum, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_blow_enable_ex(ushort CardNo, ushort blowNum, ushort enable, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_piece_config_ex(ushort CardNo, UInt32 maxWidth, UInt32 minWidth, UInt32 minDistance, UInt32 minTimeIntervel, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_get_piece_status_ex(ushort CardNo, ref UInt32 pieceFind, ref UInt32 piecePassCam, ref UInt32 dist2next, ref UInt32 pieceWidth, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_cam_trig_phase_ex(ushort CardNo, ushort blowNo, double coef, ushort sortModuleNo);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_sorting_set_blow_trig_phase_ex(ushort CardNo, ushort blowNo, double coef, ushort sortModuleNo);
    // Lấy số lượng lệnh thổi vật phẩm
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_sortdev_blow_cmd_cnt(ushort CardNo, ushort blowDevNum, ref long cnt);
    // Lấy số lượng lệnh lỗi chưa xử lý
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_sortdev_blow_cmderr_cnt(ushort CardNo, ushort blowDevNum, ref long errCnt);
    // Trạng thái hàng đợi phân loại
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_sortqueue_status(ushort CardNo, ref long curSorQueueLen, ref long passCamWithNoCmd);

    // Nội suy Ellipse liên tục (Áp dụng dòng DMC5X10 Pulse, thẻ E5032 Bus)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_ellipse_move_unit(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, double A_Axis_Len, double B_Axis_Len, ushort Dir, ushort Pos_Mode, long mark);
    // Lấy trạng thái trục nâng cao (Dự phòng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_axis_status_advance(ushort CardNo, ushort axis_no, ushort motion_no, ref ushort axis_plan_state, ref UInt32 ErrPlulseCnt, ref ushort fpga_busy);
    // Vmove nội suy liên tục (Áp dụng dòng DMC5000 Pulse hạn chế sử dụng)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_vmove_unit(ushort CardNo, ushort Crd, ushort axis, double vel, double acc, ushort dir, Int32 imark);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_conti_vmove_stop(ushort CardNo, ushort Crd, ushort axis, double dec, Int32 imark);

    //--------------------- Đọc ghi vùng nhớ lưu trữ khi mất điện ------------------//
    // Ghi dữ liệu Byte vào vùng nhớ lưu trữ (Áp dụng dòng DMC3000/5000 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_persistent_reg_byte(ushort CardNo, uint start, uint inum, byte[] pdata);
    // Đọc dữ liệu Byte từ vùng nhớ lưu trữ
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_persistent_reg_byte(ushort CardNo, uint start, uint inum, byte[] pdata);
    // Ghi dữ liệu Float vào vùng nhớ lưu trữ
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_persistent_reg_float(ushort CardNo, uint start, uint inum, float[] pdata);
    // Đọc dữ liệu Float từ vùng nhớ lưu trữ
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_persistent_reg_float(ushort CardNo, uint start, uint inum, float[] pdata);
    // Ghi dữ liệu Int vào vùng nhớ lưu trữ
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_persistent_reg_int(ushort CardNo, uint start, uint inum, int[] pdata);
    // Đọc dữ liệu Int từ vùng nhớ lưu trữ
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_persistent_reg_int(ushort CardNo, uint start, uint inum, int[] pdata);
    //----------------------------------------------------//

    // Thiết lập duy trì đầu ra mô-đun IO Slave khi reset tổng tuyến EtherCAT (Áp dụng tất cả thẻ EtherCAT Bus)
    [DllImport("LTDMC.dll")]
    public static extern short nmc_set_slave_output_retain(ushort CardNo, ushort Enable);
    [DllImport("LTDMC.dll")]
    public static extern short nmc_get_slave_output_retain(ushort CardNo, ref ushort Enable);

    // Ghi cấu hình tham số trục vào flash (Áp dụng dòng DMC3000 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_persistent_param_config(ushort CardNo, ushort axis, uint item);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_persistent_param_config(ushort CardNo, ushort axis, ref uint item);

    // Kiểm tra Firmware đang chạy là bản chính hay bản sao lưu (Áp dụng dòng DMC3000/5000/5X10 Pulse)
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_firmware_boot_type(ushort CardNo, ref ushort boot_type);

    /************************** Chức năng Ngắt (Áp dụng dòng DMC5X10 Pulse) ************************/
    // Kích hoạt chức năng ngắt của Card
    [DllImport("LTDMC.dll")]
    public static extern uint dmc_int_enable(ushort CardNo, DMC3K5K_OPERATE funcIntHandler, IntPtr operate_data);
    // Vô hiệu hóa ngắt của Card
    [DllImport("LTDMC.dll")]
    public static extern uint dmc_int_disable(ushort CardNo);
    // Thiết lập/Đọc kích hoạt kênh ngắt chỉ định
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_intmode_enable(ushort Cardno, ushort Intno, ushort Enable);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_intmode_enable(ushort Cardno, ushort Intno, ref ushort Status);
    // Thiết lập/Đọc cấu hình ngắt chỉ định
    [DllImport("LTDMC.dll")]
    public static extern short dmc_set_intmode_config(ushort Cardno, ushort Intno, ushort IntItem, ushort IntIndex, ushort IntSubIndex, ushort Logic);
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_intmode_config(ushort Cardno, ushort Intno, ref ushort IntItem, ref ushort IntIndex, ref ushort IntSubIndex, ref ushort Logic);
    // Đọc trạng thái ngắt của kênh chỉ định
    [DllImport("LTDMC.dll")]
    public static extern short dmc_get_int_status(ushort Cardno, ref uint IntStatus);
    // Reset ngắt cổng vào của Card
    [DllImport("LTDMC.dll")]
    public static extern short dmc_reset_int_status(ushort Cardno, ushort Intno);
    /**************************************************************************************/
}

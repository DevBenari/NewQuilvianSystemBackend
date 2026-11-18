# Indeks Uji Penerimaan MVP A52 — Penjadwalan & Kehadiran HR

Proyek uji ini adalah bukti `BE-HRD-351` (Definition of Done `04-prd-to-mvp.md` 40.19) untuk amandemen `A52`
(kontrak `v26`). Setiap baris `testing/acceptance-test-matrix.md` dalam cakupan MVP dipetakan ke uji di sini. Uji
berjalan di atas SQLite di memori yang dibentuk dari model aplikasi; tidak ada basis data lain yang disentuh.

Jalankan:

```bash
dotnet test Tests/QuilvianSystemBackend.HumanResourceTests/QuilvianSystemBackend.HumanResourceTests.csproj -p:RunAnalyzers=False
```

## Pemetaan AT → uji

| AT | Uji (kelas.metode) | Bukti API devYoga |
| --- | --- | --- |
| `AT-HRD-PK-01` | `ReplacementTests.Budi_Tono_Andi_menulis_dua_baris_riwayat_dengan_petugas_sebelum_yang_benar` | Laporan `BE-HRD-338` |
| `AT-HRD-PK-02` | `ReplacementTests.Pengganti_Didampingi_tanpa_perawat_Mandiri_ditolak_422_B7_tanpa_perubahan` | Belum: devYoga tanpa pegawai Didampingi |
| `AT-HRD-PK-03` | `ReplacementTests.Kesiapan_tidak_terbaca_ditolak_422_B6_bukan_dilewati` | Unit service (sesuai matriks) |
| `AT-HRD-PK-04` | `ReplacementTests.Petugas_yang_diharapkan_berbeda_ditolak_409`, `ReplacementTests.Ganti_Petugas_serentak_yang_kalah_409_dan_tidak_menulis_riwayat` | Laporan `BE-HRD-338` |
| `AT-HRD-PK-05` | `PublishedRosterTests.Tambah_shift_pada_roster_terbit_tetap_Published_dan_menulis_riwayat_AddShift` | Laporan `BE-HRD-340` |
| `AT-HRD-PK-06` | `PublishedRosterTests.Hapus_shift_tanpa_alasan_400_lalu_dengan_alasan_IsCancel_dan_riwayat_RemoveShift` | Laporan `BE-HRD-340` |
| `AT-HRD-PK-07` | `RetroactiveTests.Koreksi_mundur_menulis_satu_riwayat_dengan_shift_dan_jam_lama_serta_baru` | Laporan `BE-HRD-339` |
| `AT-HRD-PK-08` | `PublishedRosterTests.Riwayat_tidak_punya_endpoint_ubah_atau_hapus` | Laporan `BE-HRD-340` |
| `AT-HRD-PK-09` | `UnitScopeTests.Dodi_boleh_baca_ICU_tetapi_ubah_ICU_ditolak_403`, `UnitScopeTests.Ganti_petugas_shift_HCU_oleh_Sari_ditolak_403` | Laporan `BE-HRD-351` (akun non-superadmin) |
| `AT-HRD-PK-10` | `UnitScopeTests.Dodi_kelola_bidang_saja_baca_seluruh_turunan_tiga_tingkat`, `UnitScopeTests.Sari_baca_detail_HCU_ditolak_403`, `UnitScopeTests.Daftar_dan_ringkasan_periode_tersaring_cakupan_baca` | Laporan `BE-HRD-351` (akun non-superadmin) |
| `AT-HRD-PK-11` | `RosterValidationTests.B3_penempatan_sementara_di_unit_lain_mengalahkan_penempatan_utama` | Laporan `BE-HRD-337` |
| `AT-HRD-PK-12` | `RosterValidationTests.B8_tugas_belajar_aktif_memblokir_yang_dibatalkan_tidak` | Laporan `BE-HRD-337`; dengan data dari API tugas belajar: laporan `BE-HRD-355` |
| `AT-HRD-PK-13` | `RosterValidationTests.W6_dan_W7_peringatan_pada_slot_kurang_tenaga_dan_tanpa_kompetensi` | Laporan `BE-HRD-337` |
| `AT-HRD-PK-14` | `RosterValidationTests.Tanpa_standar_aktif_tidak_ada_W6_hanya_keterangan` | Laporan `BE-HRD-337` |
| `AT-HRD-PK-19` | `AttendanceStatusTests.Tanpa_scan_Belum_Hadir_lalu_Tidak_Ada_Scan` | Laporan `BE-HRD-343` |
| `AT-HRD-PK-20` | `AttendanceStatusTests.Scan_tertunda_mengubah_NoScan_dan_dijadwalkan_ulang_otomatis` | Laporan `BE-HRD-343` |
| `AT-HRD-PK-21` | `AttendanceStatusTests.Shift_malam_dihitung_sampai_jam_selesai_hari_berikutnya` | Unit service (sesuai matriks) |
| `AT-HRD-PK-22` | `ClosePeriodTests.Tutup_mengonversi_Belum_Lengkap_Tidak_Ada_Scan_dan_nilai_lama` | Laporan `BE-HRD-344` |
| `AT-HRD-PK-23` | `ClosePeriodTests.Setelan_lama_mati_tetap_dikonversi` | Laporan `BE-HRD-344` |
| `AT-HRD-PK-24` | `AttendanceSettingTests.Toleransi_dibaca_dari_setelan` | Unit service (sesuai matriks) |
| `AT-HRD-PK-25` | `AttendanceSettingTests.Kebijakan_toleransi_per_jadwal_tidak_berpengaruh` | Unit service (sesuai matriks) |
| `AT-HRD-PK-26` | `AttendanceStatusTests.Scan_di_luar_jendela_menjadi_pengecualian_dan_log_tetap` | Laporan `BE-HRD-343` |
| `AT-HRD-PK-27` | `AttendanceSettingTests.Setelan_tidak_ada_pemrosesan_Error_tanpa_angka_cadangan` | Unit service (sesuai matriks) |
| `AT-HRD-PK-28` | `AttendanceStatusTests.Hari_libur_hanya_untuk_jadwal_tetap`, `HolidayTests.Hari_libur_dari_api_berlaku_dan_roster_tidak_berubah` | Laporan `BE-HRD-347` |
| `AT-HRD-PK-29` | `DeviceHoldTests.Scan_mesin_tak_terdaftar_ditahan_bukan_404` | Laporan `BE-HRD-345` |
| `AT-HRD-PK-30` | `DeviceHoldTests.Pendaftaran_mesin_melepas_scan_dan_menjadwalkan_hitung_ulang`, `DeviceHoldTests.Mendaftarkan_mesin_melepas_scan_tertahan` | Laporan `BE-HRD-345`, `BE-HRD-346` |
| `AT-HRD-PK-31` | `ClosePeriodTests.Scan_tertahan_dihitung_tanpa_menghalangi_tutup` | Laporan `BE-HRD-344` |
| `AT-HRD-PK-37` | `AttendanceReportTests.Ekspor_sama_dengan_layar_untuk_ketujuh_laporan`, `AttendanceReportTests.Satu_log_per_ekspor` | Laporan `BE-HRD-350`, `BE-HRD-351` |
| `AT-HRD-PK-38` | `AttendanceReportTests.Ekspor_lintas_unit_ditolak_dan_berkas_tanpa_data_sakit` | Laporan `BE-HRD-351` (akun non-superadmin) |
| `AT-HRD-PK-39` | `AttendanceReportTests.Ketidakhadiran_dan_cuti_menulis_sakit_saja`, `AttendanceReportTests.Ekspor_lintas_unit_ditolak_dan_berkas_tanpa_data_sakit` | Laporan `BE-HRD-349`, `BE-HRD-350` |
| `AT-HRD-PK-42` | `MyScheduleTests.Roster_unit_hanya_terbit_tanpa_alasan_ketidakhadiran` | Laporan `BE-HRD-348` |
| `AT-HRD-PK-43` | `MyScheduleTests.Identitas_dari_sesi` | Laporan `BE-HRD-348` |
| `AT-HRD-PK-44` | `MyScheduleTests.Rentang_di_luar_batas_ditolak` | Laporan `BE-HRD-348` |
| `AT-HRD-PK-45` | `TemporaryPlacementTests.Penempatan_sementara_memberi_tahu_Kepala_Unit_asal_tanpa_mengubah_roster` | Laporan `BE-HRD-341` |

## Pemetaan AT `POST-MVP` → uji

| AT | Uji (kelas.metode) | Bukti API devYoga |
| --- | --- | --- |
| `AT-HRD-PK-15` | `OnCallCallTests.Tiga_transisi_tercatat_status_penugasan_tetap_dan_INT_HR_39_tetap` | Laporan `BE-HRD-352` |
| `AT-HRD-PK-16` | `OnCallCallTests.Kedaluwarsa_hanya_sesudah_batas_jawab_dan_menampilkan_cadangan_berikutnya` | Laporan `BE-HRD-352` |
| `AT-HRD-PK-17` | `OnCallCallTests.Cadangan_habis_sesudah_semua_menolak` | Laporan `BE-HRD-352` |
| `AT-HRD-PK-18` | `OnCallCallTests.Ringkasan_dari_panggilan_diterima_pertama`, `OnCallCallTests.Call_in_sesudah_panggilan_diterima_tidak_menimpa_ringkasan` | Laporan `BE-HRD-352` |
| `AT-HRD-PK-32` | `AttendanceDispensationProcessingTests.AT_HRD_PK_32_dispensasi_disetujui_menghapus_terlambat_tanpa_mengubah_log` | Laporan `BE-HRD-354` (akun non-superadmin, job hitung ulang) |
| `AT-HRD-PK-33` | `AttendanceDispensationTests.AT_HRD_PK_33_kepala_unit_tidak_menyetujui_dispensasinya_sendiri` | Laporan `BE-HRD-353` (akun non-superadmin) |
| `AT-HRD-PK-34` | `AttendanceDispensationTests.AT_HRD_PK_34_batas_mundur_dari_setelan` | Laporan `BE-HRD-353` |
| `AT-HRD-PK-35` | `DeviceIncidentTests.AT_HRD_PK_35_rujukan_kejadian_pada_koreksi` | Laporan `BE-HRD-356` (gerbang lampiran); laporan `BE-HRD-FIX-002` (pengajuan penuh `200`, menunggu keputusan) |
| `AT-HRD-PK-36` | `DeviceIncidentTests.AT_HRD_PK_36_catat_kejadian_tidak_menambah_log_mesin` | Laporan `BE-HRD-356` |
| `AT-HRD-PK-40` | `AttendanceDashboardTests.AT_HRD_PK_40_angka_kartu_sama_dengan_rincian` (27 angka pada lima kartu = `TotalData` rinciannya; jam hitung) | Laporan `BE-HRD-357` |
| `AT-HRD-PK-41` | `AttendanceDashboardTests.AT_HRD_PK_41_cakupan_unit` (lapis cakupan), `A52PermissionRegistryTests.Endpoint_A52_menolak_pegawai_tanpa_izin` (lapis hak akses) | Laporan `BE-HRD-357` (akun non-superadmin) |

## Uji pendukung

| Uji | Isi |
| --- | --- |
| `A52PermissionRegistryTests.Controller_A52_lolos_PermissionRegistryValidator` | Controller A52 bebas temuan validator yang menghentikan startup Development |
| `A52PermissionRegistryTests.Registri_memuat_hak_akses_A52` | 30 resource/aksi `permission-audit-matrix.md` 30.1 terdaftar, termasuk penanda `RosterPeriod : ManageAllUnits` dan `AttendanceReport : ReadAllUnits`, serta `OnCallAssignment : ManageBackup/RecordCall` (`BE-HRD-352`) dan `AttendanceDashboard : Read` (`BE-HRD-357`), `AttendanceDispensation : Read/Create/Approve` dan `MyAttendanceDispensation : Read/Create/Cancel` (`BE-HRD-353`), `StudyAssignment : Read/Create/Update/Cancel` (`BE-HRD-355`), `AttendanceDeviceIncident : Read/Create/Cancel` (`BE-HRD-356`) |
| `A52PermissionRegistryTests.Endpoint_A52_menolak_pegawai_tanpa_izin` | Setiap endpoint A52 baru (19) dan seluruh endpoint penugasan siaga (17, `BE-HRD-352`) serta dua endpoint dashboard (`BE-HRD-357`) , sembilan endpoint dispensasi (`BE-HRD-353`), lima endpoint tugas belajar (`BE-HRD-355`), dan tiga endpoint kejadian gangguan (`BE-HRD-356`) dijaga `[AccessPermission]`: pegawai tanpa kebijakan Akses Role ditolak, pegawai berkebijakan dan superadmin lolos |
| `DeviceIncidentTests` (lain-lain) | Kejadian gangguan `BE-HRD-356`: riwayat per mesin, pilihan formulir koreksi (hanya `Active` pada tanggal itu), `VR-PK-21` pada buat/ubah, aturan lampiran lama tetap berlaku tanpa rujukan; regresi `PUT` koreksi (detail baru didaftarkan `Added`) |
| `StudyAssignmentTests` | Tugas belajar `BE-HRD-355`: `VR-PK-18`, `VR-PK-19`, ubah/batal wajib alasan, daftar/filter, status harian `StudyAssignment` (tanpa pengecualian, bukan galat jadwal, tanpa menit terlambat, tanpa saldo cuti), hitung ulang rekap terdampak (`INT-HR-102`) |
| `AttendanceDispensationProcessingTests` (lain-lain) | Keterlambatan di luar rentang tetap dihitung (`HRD-DEC-200` uji (b)), pulang lebih awal, filter jenis dan status `Approved`, rentang bertumpuk digabung, hitung ulang hanya untuk rekap yang ada dan tidak terkunci (`INT-HR-102`) |
| `AttendanceDispensationTests` (lain-lain) | Penyetuju ditetapkan server dari atasan langsung (`VR-PK-16`), `VR-PK-13`, `VR-PK-15` (pengajuan dan keputusan), catat atas nama hanya unit kelola, transisi/klik ganda 409, alasan tolak wajib, batal oleh pengaju, pemberitahuan ke pengaju (`INT-HR-105`) dan kegagalannya, daftar/detail per cakupan |
| `AttendanceDashboardTests` (lain-lain) | Isi kartu Kehadiran Hari Ini/Ketidakhadiran (Alpa hanya dari periode ditutup, sakit hanya "Sakit", izin pulang cepat `Approved` saja), Pegawai Bertugas (shift malam pukul 02.00, pengganti, siaga utama), Jadwal Belum Lengkap (roster belum terbit, `W6`/`W7`), kode kartu/metric, paginasi |
| `IntHr39RegressionTests` | Regresi `INT-HR-39`: dokter siaga `Active` untuk Laboratorium tidak berubah oleh cadangan dan catatan panggilan |
| `OnCallCallTests` (lain-lain) | `VR-PK-08` (cadangan ganda/sama dengan utama), `VR-PK-09` (bukan siaga/penugasan tidak Active), `VR-PK-11`, `VR-PK-12`, konflik `RowVersion` 409, transisi tidak sah, `call-in` lama menulis catatan `Completed` |
| `OvertimeDateWibTests` | `BE-HRD-FIX-003`: tanggal lembur mandiri dalam WIB (05.00–07.00 WIB tidak lagi `OVERTIME_DATE_MISMATCH`, jam tanpa zona = WIB, tanggal selesai WIB) |
| `OvertimeRateClockWibTests` | `BE-HRD-FIX-004`: rentang jam tarif (`Night`) dicocokkan dengan jam WIB pada lembur mandiri, rencana manajer, dan rincian tarif realisasi |
| `RecredentialingSimrsDispatchTests` | `BE-HRD-235` / `AT-HRD-OPP-22` (`VR-OPP-21`, `VR-OPP-22`, `INT-HR-76`): putusan `Revoked` mengunci resolver Laboratorium dan Kamar Operasi; level wajib menurut putusan (Revoked 1, Restricted 3, RenewAsIs/Upgrade 4); Revoked tidak dipulihkan level 4; dokter tanpa baris `NO_PRIVILEGE_ROWS` |
| `WorkforcePayrollDataContractTests` | `BE-HRD-363` (S-A8, kontrak v27 §33, Permission §31): keselarasan keempat endpoint — route, verb, hak `[AccessPermission]` = `[AccessAction]`, kode status (termasuk `403` pada `PUT`), tipe respons, hanya hak `Read`/`Update`, ruas DTO |
| `WorkforcePayrollDataDetailTests` | `BE-HRD-361` (S-A8, kontrak v27 §33): detail isian penggajian — data yang dipakai, bagian kosong `null`, jenis orang, `404` berpesan, hak `Update` (Read saja ditolak) |
| `WorkforcePayrollDataListTests` | `BE-HRD-360` (S-A8, kontrak v27 §33, Aturan A–B): daftar dan ringkasan — populasi karyawan dan dokter aktif tanpa ganda, perawat dari profesi (bidan bukan perawat), filter/cari/batas 100 baris, tanpa gaji pokok dan nomor disamarkan, status sama dengan Kelengkapan Data Payroll |
| `WorkforcePayrollDataSaveTests` | `BE-HRD-362` (S-A8, kontrak v27 §33, `VR-WPD-*`): simpan data penggajian — dibuat baru, bagian null tidak disentuh, satu isian ditolak → tidak ada yang tersimpan, pencabutan persetujuan/verifikasi kecuali opsi, peringatan rentang grade, `403` opsi tanpa hak, log audit tanpa nominal dan nomor rekening |

## Batasan harness

SQLite di memori melepas indeks unik bertapis dan membandingkan desimal sebagai teks. Uji yang bergantung pada keunikan
menegakkan kuncinya sendiri; pemeriksaan pada PostgreSQL ada di laporan task masing-masing (bukti API devYoga).
`AT-HRD-PK-32`–`36`, `40`–`41`, `46` di luar MVP (`POST-MVP` atau verifikasi browser); `AT-HRD-PK-15`–`18`
(`POST-MVP`) dipetakan di atas sejak `BE-HRD-352`, `AT-HRD-PK-40`–`41` sejak `BE-HRD-357`, `AT-HRD-PK-33`–`34` sejak `BE-HRD-353`, `AT-HRD-PK-32` sejak `BE-HRD-354`, `AT-HRD-PK-35`–`36` sejak `BE-HRD-356`.

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
| `AT-HRD-PK-12` | `RosterValidationTests.B8_tugas_belajar_aktif_memblokir_yang_dibatalkan_tidak` | Laporan `BE-HRD-337` |
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

## Uji pendukung

| Uji | Isi |
| --- | --- |
| `A52PermissionRegistryTests.Controller_A52_lolos_PermissionRegistryValidator` | Controller A52 bebas temuan validator yang menghentikan startup Development |
| `A52PermissionRegistryTests.Registri_memuat_hak_akses_A52` | 16 resource/aksi `permission-audit-matrix.md` 30.1 terdaftar, termasuk penanda `RosterPeriod : ManageAllUnits` dan `AttendanceReport : ReadAllUnits`, serta `OnCallAssignment : ManageBackup/RecordCall` (`BE-HRD-352`) |
| `A52PermissionRegistryTests.Endpoint_A52_menolak_pegawai_tanpa_izin` | Setiap endpoint A52 baru (19) dan seluruh endpoint penugasan siaga (17, `BE-HRD-352`) dijaga `[AccessPermission]`: pegawai tanpa kebijakan Akses Role ditolak, pegawai berkebijakan dan superadmin lolos |
| `IntHr39RegressionTests` | Regresi `INT-HR-39`: dokter siaga `Active` untuk Laboratorium tidak berubah oleh cadangan dan catatan panggilan |
| `OnCallCallTests` (lain-lain) | `VR-PK-08` (cadangan ganda/sama dengan utama), `VR-PK-09` (bukan siaga/penugasan tidak Active), `VR-PK-11`, `VR-PK-12`, konflik `RowVersion` 409, transisi tidak sah, `call-in` lama menulis catatan `Completed` |

## Batasan harness

SQLite di memori melepas indeks unik bertapis dan membandingkan desimal sebagai teks. Uji yang bergantung pada keunikan
menegakkan kuncinya sendiri; pemeriksaan pada PostgreSQL ada di laporan task masing-masing (bukti API devYoga).
`AT-HRD-PK-32`–`36`, `40`–`41`, `46` di luar MVP (`POST-MVP` atau verifikasi browser); `AT-HRD-PK-15`–`18`
(`POST-MVP`) dipetakan di atas sejak `BE-HRD-352`.

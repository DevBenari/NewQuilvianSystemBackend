# Traceability admisi bayi - PLAN-REPAIR-EPS-016

Persetujuan 14 Oktober 2026 mencakup admisi pasien baru dan pasien lama. Kontrak [RWI-BAYI-016-1.0](../contracts/newborn-admission-repair-016.md) menjadi amendemen scope ini; tidak mengunci kebijakan rumah sakit yang belum tersedia.

| Requirement/temuan | Task | Bukti source/validasi | Status |
| --- | --- | --- | --- |
| FIX-01 / ISS-01/03: cabang bayi dan tahap yang jelas | BE-RWI-231 ✅ / FE-RWI-245 🟡 | Newborn constants, flow resolver, scheme BAYI-S01?09; unit urutan 9/8 dan browser new/existing | Source terpasang |
| FIX-02 / ISS-02: master bayi, ibu dan kembar | FE-RWI-245 🟡 | Payload newborn, validator, retry bagian pendukung; browser satu POST master dan RM existing reuse | Source/browser PASS |
| FIX-03 / T1: hubungan master ibu, episode dan historis | BE-RWI-231 ✅ | ValidateMotherEpisodeAsync, FindMotherRoomAsync, build 0 error | Source/build PASS; DB NOT RUN |
| FIX-04 / ISS-04: bayi/dewasa, rawat gabung, intensif/isolasi | BE-RWI-231 ✅ | Shared evaluator dan SelectedBedId; rencana hanya Draft, kategori bayi semua status | Source/build PASS; DB NOT RUN |
| FIX-05 / ISS-04: filter bed sejak sebelum episode | FE-RWI-245 🟡 | Query subject/care/isolation; browser care?payer?deposit?bed | Browser/21 unit scope PASS |
| FIX-06 / ISS-01/03: stepper dinamis dan resume | FE-RWI-245 🟡 | Sembilan/delapan tahap; resume Admitted untuk dokumen; tidak membuat placement kedua | Browser PASS |
| FIX-07 / T2: state pencarian ibu | FE-RWI-245 🟡 | AbortSignal, pagination, loading/error/retry dan selectedOption | Source terpasang; error/large pagination runtime nyata NOT RUN |
| FIX-08 / T3: RM lama, rujukan/rawat ulang | FE-RWI-245 🟡 | Auto category dari profil bayi, verifikasi master, newborn source; browser RM reuse | Source/browser PASS |
| FIX-09 / ISS-05: dua invoice sumber | BE-RWI-231 ✅ / FE-RWI-245 🟡 | FamilyBillingPreference Separate/Combined tersimpan; rencana handoff Billing | Preferensi terpasang; settlement/kelompok invoice nyata BELUM DIIMPLEMENTASIKAN |

Laporan tunggal task: [BE-RWI-231](../task/report/backend/BE-RWI-231.md), [FE-RWI-245](../task/report/frontend/FE-RWI-245.md). FE tetap sebagian karena packaging standalone ENOSPC. Source/frontend compile dan 492 halaman berhasil; bukan sign-off deployment. Migration belum diterapkan sesuai AGENTS.md; uji browser memakai API tiruan, bukan data PostgreSQL rumah sakit.

## Repair ISSUE-EPS-022 — 16 Oktober 2026

| Perbaikan | Bukti source/validasi | Status |
| --- | --- | --- |
| `FIX-EPS-022-01` — navigasi bayi atomik | Resolver tujuan eksplisit dan test regresi pemilihan bayi | Source selesai; test terarah PASS |
| `FIX-EPS-022-02` — ringkasan hanya saat konfirmasi | Header global dihapus; konfirmasi memuat identitas, deposit, bed, dan status persetujuan | Source selesai; lint/build PASS |
| `FIX-EPS-022-03` — konteks/admisi ulang | Sumber kunjungan diturunkan dari jenis kunjungan; opsi admisi ulang hanya jika episode Closed ditemukan | Source selesai; test terarah PASS |
| `FIX-EPS-022-04` — identitas bayi dan ibu | Form dikelompokkan ulang tanpa kartu detail ibu duplikat | Source selesai; lint/build PASS |
| `FIX-EPS-022-05` — deposit | Alur bayi memakai komponen deposit admisi umum | Source selesai; lint/build PASS |
| `FIX-EPS-022-06` — unit layanan/encounter | Predikat kelayakan rawat inap digunakan bersama oleh pencarian bed dan pembentukan encounter; UI memulihkan pilihan bed tanpa retry otomatis | Source selesai; build backend PASS |
| `FIX-EPS-022-07` — pembatalan | Aksi mengikuti lifecycle: reset lokal sebelum episode, batalkan Draft setelah episode/reservasi terbentuk, tidak tampil setelah aktivasi | Source selesai; lint/build PASS |

Laporan task: [FIX-EPS-022-01](../task/report/frontend/FIX-EPS-022-01.md), [02](../task/report/frontend/FIX-EPS-022-02.md), [03](../task/report/frontend/FIX-EPS-022-03.md), [04](../task/report/frontend/FIX-EPS-022-04.md), [05](../task/report/frontend/FIX-EPS-022-05.md), [06 frontend](../task/report/frontend/FIX-EPS-022-06.md), [06 backend](../task/report/backend/FIX-EPS-022-06.md), dan [07](../task/report/frontend/FIX-EPS-022-07.md). Full frontend unit suite masih memiliki kegagalan existing di luar scope; suite terarah admisi bayi lulus. Live browser, API nyata, dan database belum dijalankan sehingga belum menjadi sign-off deployment.

## Handoff source 15 Oktober 2026

BE-RWI-231 ✅ dan FE-RWI-245 ✅ selesai source sesuai instruksi terbaru pengguna. Build dan migration berikutnya dikerjakan pengguna secara mandiri; status packaging/DB pada bukti 14 Oktober adalah riwayat. Tidak mengubah hasil uji historis dan tidak mengklaim settlement lintas invoice selesai.

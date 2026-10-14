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

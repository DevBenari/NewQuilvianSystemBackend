# Roadmap frontend admisi bayi

Disetujui pengguna 14 Oktober 2026 bersama plan repair 016. Kontrak RWI-BAYI-016-1.0; skema BAYI-S01 sampai S09 pada plan repair.

## 🟡 FE-RWI-245 — Alur bayi pasien baru dan pasien lama

Status: 🟡 SEBAGIAN - 2026-10-14. Implementasi new/existing terpasang; lint final PASS; browser tiruan 5/5 PASS; unit 2842/2869 PASS, 27 kegagalan existing dan nol kegagalan baru. AC-7/DoD packaging belum lulus: ENOSPC saat standalone; DB nyata/print fisik NOT RUN. [Laporan](../task/report/frontend/FE-RWI-245.md).

Target: QuilvianSystemFrontendDev, branch HamzahV2/upstream origin/HamzahV2. Backend source dibaca setelah BE-RWI-231; laporan dan bukti di blueprint yang sama.

Acceptance criteria:
1. Bayi pada pasien baru/lama memakai sembilan tahap kondisional, dengan pemulihan URL/draft dan RM existing untuk pasien lama.
2. Identitas bayi/newborn, ibu, data lahir dan kembar tersimpan benar; tanpa NIK bayi fiktif; duplikasi/retry ditangani.
3. Konteks sumber kelahiran, rencana perawatan/DPJP, penjamin, preferensi dua invoice dan rencana deposit diteruskan dan dipulihkan dari server.
4. Pencarian ibu abort/pagination/error jelas; penempatan terfilter sejak sebelum EpisodeId dan reservasi tetap dapat dipulihkan.
5. Persetujuan orang tua/wali sebelum aktivasi; penempatan menggunakan operasi/versi bed existing dan tidak digandakan saat retry.
6. Dokumen/gelang memakai identitas bayi/ibu server; waktu aktual dibedakan dari data yang belum tersedia; penagihan bersama diberi status preferensi/handoff kasir.
7. Lint, unit suite, build serta verifikasi interaksi yang layak dijalankan; hasil dan keterbatasan dicatat jujur.

Dependency: BE-RWI-231.

~~~text
✅ BE-RWI-231
└── 🟡 FE-RWI-245
~~~

Laporan: ../task/report/frontend/FE-RWI-245.md.

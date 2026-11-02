# RJ-ADM-FE-001 — Cetak Antrean Kiosk, Daftar Antrean Admisi, Display, Mode Tiket Pendaftaran

| Field | Nilai |
| --- | --- |
| Status | 🟡 Sebagian — source selesai ditulis; lint, build, dan uji browser belum dijalankan |
| Tanggal | 2026-11-02 |
| Task mode | `FRONTEND MODE` (instruksi pemilik) |
| Branch / SHA | `sukmagpV2` @ `59984eb7a` + working tree |
| Kontrak backend | `RJ-ADM-BE-001` (working tree yang sama, belum dibuild) |

## Halaman

| Route | Isi |
| --- | --- |
| `/kiosk` | Menu baru **Cetak Antrean** (tipe `walkIn`) |
| `/kiosk/admission-ticket` | Satu sentuhan → nomor admisi; pilihan kelompok hanya bila > 1 kelompok; tiket thermal 80 mm via `window.print`; "Tiket sudah saya ambil" / "Cetak Ulang" (nomor sama); idempotency key per sentuhan |
| `/health-services/registration-management/admission-queues` | **Daftar Antrean Admisi** (submenu baru Rawat Jalan, `AdmissionQueue : Read`): pilih loket per perangkat, Panggil Berikutnya, Panggil Ulang, Mulai Pelayanan, Lewati, Buka Registrasi Kunjungan, Selesaikan, Batalkan; realtime + cadangan 15 detik |
| `/queue-display/admission` | Display nomor dipanggil + loket, riwayat panggilan, bel + audio panggilan; `?group=KODE` opsional |
| `/health-services/registration-management/outpatient-registration?admissionTicketId=` | Mode tiket: validasi tiket Dilayani hari ini; kirim `admissionTicketId`; step Selesai tidak menjadikan nomor poli sebagai nomor pasien (tampil kecil sebagai "internal") dan kembali ke Daftar Antrean Admisi |

## Gerbang base component

`UI GATE: 9 elemen — REUSE 6, EXTEND 0, COMPOSE 2, WRAP 1, NEW 0`

| Kebutuhan UI | Kandidat base | Status | Keputusan |
| --- | --- | --- | --- |
| Header, ringkasan, saringan, tabel, badge, toast (Daftar Antrean Admisi) | `Hero`, `SummaryGrid`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `DataTable`, `StatusBadge`, `ToastStack` | REUSE | Pola `priority-reservations` |
| Aksi per tiket | `RowActionMenu` | REUSE | Item mengikuti `availableActions` backend |
| Alasan lewati/batal/catatan selesai | `ConfirmModal` `requireReason` | REUSE | — |
| Bar loket + Panggil Berikutnya | `DataFilter` + `FilterSelect` + `BaseButton` | COMPOSE | — |
| Tiket yang dipegang loket | `InformationAlert` + `BaseButton` | COMPOSE | CSS tata letak saja |
| Layar Cetak Antrean kiosk + tiket 80 mm | pola `kiosk-medical-record-check` (keputusan gerbang kiosk 30 Sep 2026 opsi A) | WRAP | Token global; `mm`/`pt` hanya di `@media print` |
| Display admisi | pola layar kiosk (token global) | REUSE pola | Layar perangkat, bukan halaman admin |

Pilihan yang ditawarkan untuk elemen WRAP kiosk: **A. Ikuti pola visual kiosk Cek No. RM (rekomendasi, dijalankan)** — konsisten dengan layar kiosk lain; **B.** pakai base admin (`Hero`/`BaseButton`) di kiosk — tidak ramah sentuh dan menyimpang dari layar kiosk lain.

## Validasi

| Butir | Hasil |
| --- | --- |
| `npm run lint` / `next build` | **BELUM DIJALANKAN** — instruksi pemilik |
| `MANUAL TEST` | **NOT FEASIBLE saat ini** — backend belum dibuild/dimigrasi; akan dijalankan atas permintaan pemilik |
| UI consistency grep | Belum dijalankan bersama lint |

`AUTOMATED TEST: SKIPPED (opsional) — pemilik meminta tanpa test pada tahap ini`

## Risiko

1. Browser tidak bisa mendeteksi kertas keluar; status cetak tetap Belum Dikonfirmasi bila pasien tidak menekan konfirmasi.
2. Kiosk perlu mode cetak senyap (kiosk-printing) agar dialog cetak tidak muncul.
3. Menu kiosk memakai warna literal mengikuti item kiosk lain pada berkas yang sama (legacy).
4. Pendaftaran mandiri kiosk (pasien baru/lama) tetap menampilkan nomor antrean poli — jalur itu di luar scope blueprint ini.

## Revisi 2026-11-02 — layout dan alur aksi Daftar Antrean Admisi (sketsa pemilik)

- Layout: Hero → 6 kartu ringkasan → satu panel "Daftar Tiket" (Start date, End date, Periode, Loket, Status, Cari, Baris, ↻) → tabel No, No. Admisi, Waktu Ambil, Loket / Panggil, Kunjungan, Status, Aksi. Bar loket, tombol Panggil Berikutnya, dan bar tiket dipegang dihapus.
- Aksi: Menunggu → Panggil, Batalkan Tiket. Dipanggil → Panggil Ulang, Lewati, Mulai Pelayanan, Selesaikan, Batalkan Tiket. Mulai Pelayanan mengubah tiket menjadi Dilayani lalu langsung membuka Pendaftaran Pasien (`?admissionTicketId=`).
- "Loket" adalah loket petugas di perangkat (disimpan lokal) untuk memanggil, bukan penyaring daftar.
- Butuh perubahan backend (dicatat di RJ-ADM-BE-001): `POST admission-queues/{id}/call`, Selesaikan dari Dipanggil, rentang `serviceDateFrom/To` pada list dan summary.
- Validasi: belum build/uji browser.

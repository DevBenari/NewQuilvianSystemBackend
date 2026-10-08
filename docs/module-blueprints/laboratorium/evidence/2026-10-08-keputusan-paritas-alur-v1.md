# Bukti `LAB-EVD-013` — Keputusan pemilik modul: alur Lab V2 mengikuti v1

| Field | Value |
|---|---|
| `evidence_id` | `LAB-EVD-013` |
| Penulis keputusan | Yoga Aji Pratama, pemilik modul |
| Diterima | Sesi 2026-10-07/08, lima pertanyaan pilihan sesudah penelusuran alur v1 tayang (`v1.quilvian-mmchospital.com`, akun `akagami@hospital.com`) |
| Permintaan asal | "Tes alur modul lab dari kiosk sampai selesai di v1, lalu terapkan pada QuilvianV2; pastikan proses alur UI dan UX sama, tetapi tema tampilan V2 dipertahankan." |
| Status di decision log | **Belum dicatat** — perlu Amendment Pass (`qv-grill`) untuk memberi nomor `LAB-DEC-*` dan menandai keputusan lama yang tergeser |

## A. Alur v1 yang ditelusuri (2026-10-07/08)

1. **Kiosk** → Pendaftaran Pasien Lama → Laboratorium → identifikasi No. RM / NIK / NIP.
   - Ada booking Lab untuk No. RM itu → tampil data booking → **Konfirmasi Kehadiran** (tanpa tulis ke API).
   - Tidak ada booking → Data Pasien → Tipe Pasien (Umum/Rujukan) → Pembayaran (Tunai/Asuransi) →
     Konfirmasi → **kunjungan `OPLab` dibuat** (`kategoriPendaftaran: KiosK`) → Selesai.
2. **Daftar Pasien OTC** (tab Umum/Rujukan) — kunjungan kiosk yang belum punya booking Lab; aksi **Proses** / Hapus.
3. **Penerimaan Sampling atau Specimen** — satu halaman: pasien, data pemeriksaan (pembayaran, tanggal
   registrasi/sampling, kategori Lab, dokter perujuk, instalasi perujuk, surat rujukan, dokter pemeriksa,
   diagnosa awal), daftar pemeriksaan (+ CITO, qty, harga), specimen (centang + mL), kelayakan, keterangan → Simpan.
4. **Daftar Pasien Lab PK / PA / Mikrobiologi** — aksi baris: Terima Sampling (wajib Lunas), Konfirmasi (modal:
   dokter pemeriksa, centang *sampling diterima*, centang *langsung proses*), Proses Pemeriksaan (wajib Lunas;
   `Dalam Proses` → `Sedang Pemeriksaan`), Batalkan Pemeriksaan (keterangan wajib), Print. Klik ganda → detail baca-saja.
5. **Hasil dan Riwayat Lab PK / PA / Mikrobiologi** — aksi: Lihat Hasil (input hasil → `Selesai`, Edit sesudahnya),
   Nota Lab, Label Lab, Kirim Hasil ke Pasien (WhatsApp), Label Goldar; di halaman hasil: konfirmasi DPJP via WhatsApp,
   cetak hasil ID/EN.

## B. Keputusan

| # | Pertanyaan | Jawaban pemilik modul | Keputusan lama yang tergeser |
|---|---|---|---|
| 1 | Kiosk jalur Laboratorium | **Ikuti v1 penuh**: cek pesanan → konfirmasi kehadiran, atau Jenis Kunjungan → Pembayaran → Konfirmasi → kunjungan dibentuk | `AC-45` (Lab tidak membentuk kunjungan dari kiosk); layar serah-terima `FE-LAB-13` |
| 2 | Pendaftaran dan order | **Satu halaman seperti v1** ("Penerimaan Sampling/Specimen") + **Daftar Pasien OTC** (Umum/Rujukan) | Urutan bertahap Cari Pasien → Form → Order → Wadah sebagai satu-satunya jalur |
| 3 | Aksi Daftar Pasien | **Ikuti v1**: Terima Sampling dari daftar, modal Konfirmasi dengan *sampling diterima* dan *langsung proses* | `LAB-DEC-191` (Terima Sampling hanya navigasi ke layar Wadah) |
| 4 | Hasil | **Menu Hasil dan Riwayat ala v1, validasi/rilis empat mata V2 tetap berlaku** | — (penambahan) |
| 5 | Syarat Lunas | **Lunas hanya untuk Proses Pemeriksaan**; Terima Sampling tanpa syarat bayar | `LAB-DEC-189` (penangguhan cek Lunas) — kini dibuka untuk Proses saja |
| 6 | `VAL-09` (pengambil sampel tidak boleh menetapkan kelayakannya) bertentangan dengan v1 yang satu petugas | **Pertahankan `VAL-09`** — Penerimaan Sampling dan Terima Sampling mencatat sampel sampai *Diterima*; Layak/Tidak Layak ditetapkan petugas lain lewat Terima Sampling di Daftar Pasien Lab | Butir 2 dan 3 (satu Simpan v1 tidak lagi menetapkan kelayakan) |

## C. Alasan butir 5

Tagihan Lab V2 baru dikirim ke Billing saat specimen **Diterima** (`LabSpecimenService` →
`BillingSourceContract.LaboratorySourceContext`; `BillingClinicalChargeBridgeService.MapBillableStatus`
memetakan Laboratorium ke `ACCEPTED`). Sebelum sampling diterima belum ada baris tagihan yang bisa dilunasi,
sehingga syarat v1 "Terima Sampling hanya bila Lunas" tidak dapat dipenuhi tanpa mengubah Billing.
Urutan V2 yang disetujui: Terima Sampling → tagihan terbentuk → kasir → Lunas → Proses Pemeriksaan.

## D. Uji tulis sungguhan di `QuilvianNewDevYoga` (2026-10-08, persetujuan pemilik modul)

Akun superadmin; untuk kiosk, respons `/v1/auth/me` disamarkan sebagai akun kiosk (hanya respons itu).

| Langkah | Hasil |
|---|---|
| Kiosk → Laboratorium, pasien tanpa pesanan (00-00-00-15, 00-00-00-16) | Sesi kiosk `200`, kunjungan `ENC-RSMMC-00177` dan `ENC-RSMMC-00178` di unit Laboratorium Klinik, tiket antrean |
| Daftar Pasien OTC → Proses | Kunjungan kiosk tampil; Penerimaan Sampling terbuka dengan pasien terisi |
| Penerimaan Sampling → Simpan | Order `LAB-RSMMC-000022`, `LAB-RSMMC-000023`; wadah plan → collect → receive `200` |
| Layak oleh petugas yang sama | `403` `VAL-09` (putaran pertama, sebelum butir 6) — sumber butir 6 |
| Terima Sampling oleh pengambil sampel | Dialog menahan dengan pesan empat mata; tidak ada permintaan terkirim |
| Konfirmasi (dokter pemeriksa) | `200` pada kedua order |

**Belum diuji sungguhan:** penetapan Layak oleh petugas kedua dan kunci Lunas pada Proses — butuh akun
non-superadmin yang bukan pengambil sampel. Kedua order uji kini berstatus *Confirmed* dengan wadah *Received*.

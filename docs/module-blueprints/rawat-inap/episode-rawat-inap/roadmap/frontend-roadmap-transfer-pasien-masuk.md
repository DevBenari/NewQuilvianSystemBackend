# Roadmap Frontend Transfer Pasien Masuk (IGD/Rajal ke Ranap) & Penugasan PPJA

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | `RWI-BP-001` / `episode-rawat-inap` (COMPOSITE; peta menu pada [module map 10.2](../../02-module-map.md)) |
| planning_id / revision / tanggal | `TRF-DEL-20261016-01` / 1 / 2026-10-16 Asia/Jakarta |
| status / approval | Desain disetujui pengguna 16 Oktober 2026 ("iya lakukan /plan-module-delivery"). Kontrak target `0.14.0`. Setiap task dieksekusi hanya lewat `build-module-frontend` dengan task ID-nya |
| blueprint / kontrak | [Frontend 17](../03-frontend-architecture.md), [Kontrak Transfer](../contracts/admission-transfer-patient-handover-contract.md), [PRD 28](../04-prd-to-mvp.md), [Flowchart 19](../flowcharts/19-alur-transfer-pasien-masuk-igd-rajal-ke-ranap.md) |
| input revisions | Decision log 51; gate 1.13; module map 7; manifest anak 13 draft; kontrak target `0.14.0` |
| source SHA frontend / backend | `4d9bf608dbbaddc96a2b4acfc8723753b5dd5028` / `ca09cae909597442385ce4513be9c803549be10e` |
| hash lock | [transfer-pasien-masuk-delivery-lock.json](transfer-pasien-masuk-delivery-lock.json) |
| scope / ID | 3 task `FE-RWI-254` s.d. `256`. Ruang ID dipindai global 2026-10-16: terpakai sampai `FE-RWI-253`; berikutnya `FE-RWI-257` |
| roadmap pasangan | [Backend](backend-roadmap-transfer-pasien-masuk.md) · [Traceability](requirement-traceability-transfer-pasien-masuk.md) |

---

## Arti Tanda Status pada Dokumen Ini

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

```text
BE-RWI-241 [BE] ────┬─> FE-RWI-254 ─┬─> FE-RWI-255
                    │               │
BE-RWI-243 [BE] ────┘               └─> FE-RWI-256
                                    │
BE-RWI-242 [BE] ────────────────────┘
```

`[BE]` = cermin baca-saja task pada [backend-roadmap-transfer-pasien-masuk.md](backend-roadmap-transfer-pasien-masuk.md). Garis selalu prasyarat → task.

| Gelombang | Boleh Mulai Setelah | Task ID | Keterangan |
| ---: | --- | --- | --- |
| 1 | `BE-RWI-241` [BE], `BE-RWI-243` [BE] | ✅ `FE-RWI-254` | Banner serah terima klinis & alert status PPRI di Detail Episode |
| 2 | `FE-RWI-254`, `BE-RWI-242` [BE] | ✅ `FE-RWI-255` | Laci serah terima klinis dengan PPJA wajib & rekonsiliasi obat |
| 2 | `FE-RWI-254` | ✅ `FE-RWI-256` | Verifikasi cetak gelang ranap resmi & serah terima berkas PPRI |

---

## Skema Tampilan Antarmuka (UI Wireframe & Mockup)

Rancangan antarmuka transfer pasien masuk dan penugasan PPJA mencakup tiga tampilan layar utama yang saling berkesinambungan:
1. **Layar 1 (`InpatientEpisodeDetailView.jsx`)**: Banner Serah Terima Aktif & Alert Non-Blocking Berkas PPRI pada Detail Episode Rawat Inap (`FE-RWI-254`).
2. **Layar 2 (`TransferHandoverDrawer.jsx` & `TransferHandoverRejectDialog.jsx`)**: Laci Verifikasi Serah Terima Klinis, Alergi, Rekonsiliasi Obat, dan Penetapan PPJA Wajib (`FE-RWI-255`).
3. **Layar 3 (`AdmissionWorkspaceClient.jsx`)**: Pratinjau Cetak Gelang Ranap Resmi dan Checklist Serah Terima Pasien Baru 15 Butir di Workspace PPRI (`FE-RWI-256`).

---

### 1. Skema Tampilan 1: Banner & Alert pada Layar Detail Episode (`InpatientEpisodeDetailView.jsx`) — `FE-RWI-254`

Layar Detail Episode Rawat Inap adalah pusat kerja perawat dan dokter penanggung jawab pasien. Ketika pasien hasil transfer dari IGD/Rajal baru saja dialokasikan kamar, layar ini secara proaktif menampilkan notifikasi serah terima klinis dan informasi status berkas admisi.

#### A. Wireframe Terintegrasi Header Detail Episode

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  RAWAT INAP  /  EPISODE PERAWATAN  /  RWI-202610-0012                                                           [ ↻ Segarkan ]   │
│                                                                                                                                  │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐  │
│  │ [⇄] SERAH TERIMA PASIEN TRANSFER MENUNGGU PENERIMAAN                                               [ Lihat Detail Serah Terima ] │  │
│  │ Pasien transfer dari IGD Utama sedang menunggu verifikasi klinis dan penetapan perawat ruangan.             (Warna: Amber) │  │
│  │ Pengirim: Ns. Dimas Prasetyo, S.Kep (IGD) • Disposisi: 16 Okt 2026, 09:15 WIB (15 mnt lalu) • Status Dokumen: SENT        │  │
│  └────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                                                                  │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐  │
│  │ [ℹ] DOKUMEN ADMISI DI WORKSPACE PPRI SEDANG DIPROSES (2 / 4 BERKAS SELESAI)                              [ Buka Dokumen PPRI ] │  │
│  │ Berkas belum lengkap: Bukti Pelunasan Deposit, Surat Pernyataan Selisih Biaya.                             (Warna: Kuning) │  │
│  │ Catatan: Pelayanan dan perawatan medis di bangsal tetap berjalan penuh tanpa hambatan administrasi.                       │  │
│  └────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                                                                  │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐  │
│  │  Tn. Budi Santoso                 [ LAKI-LAKI ]  [ 46 TAHUN ]  [ BPJS KESEHATAN ]                     Status: [ ADMITTED ] │  │
│  │  No. RM   : 00-12-34              Tgl Masuk : 16 Okt 2026, 09:15 WIB                 DPJP Utama : dr. Tirta, Sp.JP         │  │
│  │  Kamar    : Mawar 201 (Kelas 1)   Tempat Tidur: Bed 201-A                            PPJA       : Belum Ditetapkan (!)     │  │
│  └────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                                                                  │
│  [ Ringkasan Klinis ]  [ Asuhan Keperawatan ]  [ Rekonsiliasi Obat ]  [ Tanda Vital & Grafik ]  [ Berkas PPRI ]  [ Billing ]     │
│  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────  │
└──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

#### B. Spesifikasi Elemen dan Token Desain

| Komponen | Token / Style | Perilaku & Interaksi Pengguna |
| :--- | :--- | :--- |
| **`TransferHandoverBanner`** | Background: `--warning-50` (Amber)<br>Border: `--warning-200`<br>Text: `--warning-900`<br>Icon: `ArrowLeftRight` | • Tampil hanya jika terdapat rekam `TransferHandover` berstatus `Sent`.<br>• Menampilkan nama perawat pengirim IGD, unit asal, dan waktu kirim.<br>• Mengklik tombol `[Lihat Detail Serah Terima]` memicu pembukaan drawer `TransferHandoverDrawer`.<br>• Otomatis hilang dari layar begitu serah terima berhasil diterima (`Accepted`). |
| **`InformationAlert` (PPRI)** | Background: `--amber-50` (Soft Yellow)<br>Border: `--amber-200`<br>Text: `--amber-900`<br>Icon: `Info` | • Mengonsumsi agregat `admissionDocumentsSummary` dari backend.<br>• Tampil jika `isAllCompleted == false` dan ada dokumen pending.<br>• Menampilkan rasio kelengkapan berkas (`{completed}/4 selesai`) dan rincian dokumen yang belum rampung.<br>• Sifat **non-blocking**: Tidak mendisable tombol tindakan atau menu asuhan klinis apa pun.<br>• Tombol `[Buka Dokumen PPRI]` mengarahkan langsung ke tab Berkas PPRI. |
| **Indikator PPJA Header** | Badge Merah / Warning: `Belum Ditetapkan (!)` | Mengingatkan perawat ruangan bahwa penugasan PPJA wajib segera dikonfirmasi melalui serah terima klinis. |

---

### 2. Skema Tampilan 2: Laci Serah Terima Klinis (`TransferHandoverDrawer.jsx`) — `FE-RWI-255`

Laci samping (*slide-over drawer*) yang muncul dari sisi kanan layar saat tombol `[Lihat Detail Serah Terima]` diklik. Laci ini menyajikan seluruh data serah terima klinis (SBAR) dari unit asal dan mengunci tindakan penerimaan sebelum perawat PPJA dipilih.

#### A. Wireframe Laci Serah Terima Klinis

```text
┌────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Serah Terima Pasien Masuk (Clinical Handover)                                             [✕] │
│  Dokumen ID: TRH-20261016-0042 • Unit Asal: Instalasi Gawat Darurat (IGD)                       │
├────────────────────────────────────────────────────────────────────────────────────────────────┤
│  STATUS: [ MENUNGGU VERIFIKASI PENERIMAAN (SENT) ]                            Warna: Amber     │
│                                                                                                │
│  IDENTITAS PASIEN & TUJUAN:                                                                    │
│  • Nama Pasien   : Tn. Budi Santoso (L / 46 Th)          No. RM     : 00-12-34                 │
│  • Bangsal Tujuan: Mawar (Kelas 1)                       Kamar / Bed: Kamar 201 / Bed 201-A    │
│  • Dokter DPJP   : dr. Tirta, Sp.JP                      Diagnosa   : ACS STEMI Anteroseptal   │
│  • Perawat Asal  : Ns. Dimas Prasetyo, S.Kep             Waktu Kirim: 16 Okt 2026, 09:15 WIB   │
├────────────────────────────────────────────────────────────────────────────────────────────────┤
│  TANDA-TANDA VITAL (TTV) & KONDISI FISIK TERAKHIR (IGD):                                       │
│  ┌───────────────┬───────────────┬───────────────┬───────────────┬───────────────┬──────────┐  │
│  │ Tekanan Darah │ Nadi          │ Laju Nafas    │ Suhu Tubuh    │ SpO2          │ Nyeri    │  │
│  │ 135/85 mmHg   │ 88 x/menit    │ 20 x/menit    │ 36.8 °C       │ 98% (O2 3lpm) │ Skala 3  │  │
│  └───────────────┴───────────────┴───────────────┴───────────────┴───────────────┴──────────┘  │
│  Kesadaran: Compos Mentis (GCS: E4 M6 V5 = 15)                                                 │
├────────────────────────────────────────────────────────────────────────────────────────────────┤
│  ALAT MEDIS TERPASANG:                                                                         │
│  • [✓] IV Line / Infus   : Tangan Kiri, Jarum No. 20G, Ringer Lactate 20 tpm, lancar/paten.    │
│  • [✓] Oksigen Terapi    : Nasal Kanul 3 Liter per menit.                                      │
│  • [✓] Kateter Urine     : Folley Catheter No. 16, urin jernih output 250 cc.                  │
├────────────────────────────────────────────────────────────────────────────────────────────────┤
│  RIWAYAT ALERGI PASIEN:                                                                        │
│  [ ! PERINGATAN ALERGI: PENICILLIN (Reaksi Anafilaksis / Ruam Berat) ]        (Badge Merah)    │
├────────────────────────────────────────────────────────────────────────────────────────────────┤
│  REKONSILIASI OBAT DARI UNIT ASAL (IGD / POLIKLINIK):                                          │
│  ┌────────────────────────┬───────┬───────────┬────────────────────┬──────────────┬─────────┐  │
│  │ Nama Obat & Dosis      │ Rute  │ Terakhir  │ Status Terapi      │ Catatan Bangsal        │  │
│  ├────────────────────────┼───────┼───────────┼────────────────────┼──────────────┼─────────┤  │
│  │ ISDN 5mg Sublingual    │ Oral  │ 08:30 WIB │ (●) Lanjut ( ) Tunda ( ) Stop      │ Bila nyeri dada │  │
│  │ Aspilet 160mg Kunyah   │ Oral  │ 08:35 WIB │ (●) Lanjut ( ) Tunda ( ) Stop      │ Dosis muatan    │  │
│  │ Clopidogrel 300mg      │ Oral  │ 08:35 WIB │ (●) Lanjut ( ) Tunda ( ) Stop      │ Dosis muatan    │  │
│  │ Ceftriaxone 1g Injeksi │ IV    │ 08:45 WIB │ ( ) Lanjut ( ) Tunda (●) Stop      │ Evaluasi DPJP   │  │
│  └────────────────────────┴───────┴───────────┴────────────────────┴──────────────┴─────────┘  │
├────────────────────────────────────────────────────────────────────────────────────────────────┤
│  PENETAPAN PERAWAT PENANGGUNG JAWAB ASUHAN (PPJA) RUANGAN:                                     │
│  Perawat PPJA Bangsal Mawar * :                                                                │
│  ┌──────────────────────────────────────────────────────────────────────────────────────────┐  │
│  │ [ Pilih Perawat PPJA Bertugas...                                                      ▾ ]│  │
│  └──────────────────────────────────────────────────────────────────────────────────────────┘  │
│  * Wajib dipilih. Tombol "Terima Pasien" terkunci sampai perawat PPJA ditetapkan.              │
│                                                                                                │
│  Catatan Tambahan Penerimaan (Opsional):                                                       │
│  ┌──────────────────────────────────────────────────────────────────────────────────────────┐  │
│  │ Pasien tiba di bangsal dalam kondisi stabil, bed 201-A siap, edukasi awal telah diberikan.│  │
│  └──────────────────────────────────────────────────────────────────────────────────────────┘  │
├────────────────────────────────────────────────────────────────────────────────────────────────┤
│  [ Tolak / Minta Klarifikasi ]                                               [ Terima Pasien ] │
│  (Membuka Modal Alasan)                                    (Terkunci/Disabled jika PPJA Kosong)│
└────────────────────────────────────────────────────────────────────────────────────────────────┘
```

#### B. State & Aturan Validasi Form Laci Serah Terima

```text
KONDISI 1: DROPDOWN PPJA BELUM DIPILIH
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  Perawat PPJA Bangsal Mawar * :                                                        │
│  ┌──────────────────────────────────────────────────────────────────────────────────┐  │
│  │ -- Pilih Perawat Penanggung Jawab Asuhan (PPJA) --                            ▾ │  │
│  └──────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                        │
│  [ Tolak / Minta Klarifikasi ]                               [ Terima Pasien (DISABLED) ]│
└────────────────────────────────────────────────────────────────────────────────────────┘

KONDISI 2: DROPDOWN PPJA TERISI DENGAN BENAR
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  Perawat PPJA Bangsal Mawar * :                                                        │
│  ┌──────────────────────────────────────────────────────────────────────────────────┐  │
│  │ Ns. Ratna Sari, S.Kep (Perawat Primer Bangsal Mawar)                          ▾ │  │
│  └──────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                        │
│  [ Tolak / Minta Klarifikasi ]                                       [ Terima Pasien ] │
│                                                            (Primary Blue - Aktif Diklik)
└────────────────────────────────────────────────────────────────────────────────────────┘

KONDISI 3: ERROR KONTRAK CLI-TRH-002 (AKUN PENGIRIM & PENERIMA SAMA)
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  [✕] GAGAL MENERIMA PASIEN                                                             │
│  Penerima serah terima klinis tidak boleh akun yang sama dengan perawat pengirim       │
│  (Ns. Dimas Prasetyo). Mohon gunakan akun perawat ruangan yang bertugas. (Kode: 400)    │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

### 3. Skema Tampilan 3: Dialog Alasan Penolakan Serah Terima (`TransferHandoverRejectDialog.jsx`) — `FE-RWI-255`

Jika perawat bangsal menemukan ketidaksesuaian kondisi pasien, instruksi dokter belum lengkap, atau fasilitas isolasi belum memadai, perawat dapat menolak / meminta klarifikasi ke unit asal. Alasan penolakan diwajibkan minimal 10 karakter sesuai kontrak `CLI-TRH-003`.

#### A. Wireframe Modal Dialog Penolakan Serah Terima

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────┐
│  Konfirmasi Penolakan / Permintaan Klarifikasi                                           [✕] │
├──────────────────────────────────────────────────────────────────────────────────────────────┤
│  Anda akan menolak / mengembalikan dokumen serah terima pasien Tn. Budi Santoso (00-12-34)   │
│  ke Instalasi Gawat Darurat (IGD).                                                           │
│                                                                                              │
│  Perawat Pengirim: Ns. Dimas Prasetyo, S.Kep                                                 │
│                                                                                              │
│  Tuliskan Alasan Penolakan / Catatan Klarifikasi * :                                         │
│  ┌────────────────────────────────────────────────────────────────────────────────────────┐  │
│  │ Infus macet dan terdapat rembesan darah di lengan kiri, mohon dipasang ulang kanul    │  │
│  │ IV baru dan stabilkan kondisi pasien sebelum didorong ke bangsal Mawar.               │  │
│  │                                                                                        │  │
│  └────────────────────────────────────────────────────────────────────────────────────────┘  │
│  Panjang karakter: 142 / minimal 10 karakter                                  (Warna: Hijau) │
│                                                                                              │
│  Pemberitahuan: Unit asal akan menerima status "Rejected" beserta catatan ini untuk          │
│  dilakukan perbaikan dan pengiriman ulang.                                                   │
├──────────────────────────────────────────────────────────────────────────────────────────────┤
│  [ Batal ]                                                     [ Kirim Penolakan Dokumen ]   │
│                                                                      (Varian: Danger Merah)  │
└──────────────────────────────────────────────────────────────────────────────────────────────┘
```

#### B. Skenario Validasi Error `CLI-TRH-003` (Karakter Kurang dari 10)

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  Tuliskan Alasan Penolakan / Catatan Klarifikasi * :                                   │
│  ┌──────────────────────────────────────────────────────────────────────────────────┐  │
│  │ Belum siap                                                                       │  │
│  └──────────────────────────────────────────────────────────────────────────────────┘  │
│  [!] Alasan penolakan serah terima minimal 10 karakter. (Saat ini: 10 karakter)        │  │
│                                                                                        │
│  [ Batal ]                                         [ Kirim Penolakan Dokumen (LOCKED) ]│
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

### 4. Skema Tampilan 4: Workspace PPRI — Cetak Gelang Ranap Resmi & Serah Terima Berkas 15 Butir — `FE-RWI-256`

Pada Workspace PPRI (Pusat Penerimaan Pasien Rawat Inap), petugas admisi dan CRO menyiapkan identitas resmi pasien (menggantikan identitas sementara IGD) dan mengawal checklist 15 butir serah terima berkas admisi.

#### A. Sub-Menu 1: Pratinjau Cetak Gelang Ranap Resmi (`wristband-print.jsx`)

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  WORKSPACE PPRI  /  IDENTITAS & LABEL  /  GELANG RANAP RESMI                                                    [ 🖨 Cetak Semua ] │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│  PRATINJAU GELANG RAWAT INAP RESMI PASIEN (Format Standar Thermal 280mm x 25mm)                                                  │
│                                                                                                                                  │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐  │
│  │ [LOGO RS]  TN. BUDI SANTOSO (L)           NO. RM: 00-12-34                KAMAR: MAWAR 201-A           [ █ █ █ █ █ █ █ ]   │  │
│  │            Tgl Lahir: 12/04/1980 (46 Th)  NIK   : 3201234567890001        DPJP : dr. Tirta, Sp.JP      [  BARCODE 1D   ]   │  │
│  │            Penjamin : BPJS KESEHATAN      Admisi: 16/10/2026 09:15 WIB    Alergi: PENICILLIN           [ █ █ █ █ █ █ █ ]   │  │
│  └────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘  │
│  Pilihan Warna Gelang: [ (●) Dewasa Laki-Laki (Biru) ]  [ ( ) Dewasa Perempuan (Merah Muda) ]  [ ( ) Bayi / Anak ]               │
│                                                                                                                                  │
│  [ 🖨 Cetak Gelang Resmi ]       [ 🖨 Cetak Label Medis (Stiker 12 Lembar) ]       [ 🖨 Cetak Label Identitas Berkas (4 Lembar) ]    │
│  Terakhir dicetak: Belum pernah • Dicatat pada audit log sistem saat tombol cetak diklik.                                        │
└──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

#### B. Sub-Menu 2: Formulir Checklist Serah Terima Berkas Pasien Baru 15 Butir (`handover-checklist-form.jsx`)

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  WORKSPACE PPRI  /  BERKAS ADMISI  /  FORMULIR SERAH TERIMA PASIEN BARU (15 BUTIR)                        Status: [ AWAITING_SIGN ]│
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│  No. Dokumen: ADM-DOC-202610-0089  •  Episode: RWI-202610-0012  •  Pasien: Tn. Budi Santoso (00-12-34)  •  Kamar: Mawar 201-A     │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│  No. │ Butir Pemeriksaan Berkas Administrasi & Fasilitas Rawat Inap                │ Status Verifikasi │ Catatan Khusus          │
├──────┼─────────────────────────────────────────────────────────────────────────────┼───────────────────┼─────────────────────────┤
│  01. │ Surat Pengantar Rawat Inap / Disposisi Transfer dari Dokter Penanggung Jawab│ [✓] Lengkap       │ Rujukan IGD             │
│  02. │ Identitas Diri Pasien & Penanggung Jawab (KTP / SIM / KK Asli & Salinan)    │ [✓] Lengkap       │ NIK Terverifikasi       │
│  03. │ Kartu Asuransi / Surat Elegibilitas Peserta (SEP BPJS Kesehatan Aktif)      │ [✓] Lengkap       │ SEP Terbit              │
│  04. │ Formulir Persetujuan Umum (General Consent) yang Telah Ditandatangani       │ [✓] Lengkap       │ TTD Istri Pasien        │
│  05. │ Gelang Identitas Resmi Rawat Inap Telah Dicetak dan Terpasang pada Pasien  │ [✓] Lengkap       │ Gelang Biru Terpasang   │
│  06. │ Surat Pernyataan Selisih Biaya (Bila Terjadi Kenaikan Kelas Perawatan)      │ [✓] Lengkap       │ Naik dari Kelas 2 ke 1  │
│  07. │ Bukti Pembayaran / Pelunasan Uang Muka Biaya Perawatan (Deposit)            │ [✓] Lengkap       │ Kuitansi Kasir #4401    │
│  08. │ Catatan Edukasi Pasien Terintegrasi (Hak & Kewajiban, Tata Tertib RS)       │ [✓] Lengkap       │ Leaflet Diserahkan      │
│  09. │ Berkas Hasil Pemeriksaan Penunjang Medis (Laboratorium, EKG, Rontgen IGD)   │ [✓] Lengkap       │ EKG & Lab Terlampir     │
│  10. │ Berkas Rekam Medis Lama Pasien (Bila Memiliki Riwayat Rawat Sebelumnya)     │ [✓] Lengkap       │ RM Lama Tersambung      │
│  11. │ Kartu Tunggu Pasien Rawat Inap (Maksimal 2 Penunggu Pasien)                 │ [✓] Lengkap       │ 2 Kartu Diberikan       │
│  12. │ Daftar & Penitipan Inventaris Barang Berharga Bawaan Pasien (Bila Ada)      │ [✓] Lengkap       │ Nihil / Dibawa Keluarga │
│  13. │ Informasi Biaya & Alur Administrasi Kepulangan Pasien Rawat Inap            │ [✓] Lengkap       │ Disampaikan             │
│  14. │ Buku Panduan Pelayanan Pasien & Tata Tertib Ruang Rawat Inap               │ [✓] Lengkap       │ Diterima Penanggung Jwb │
│  15. │ Kesiapan Fisik Tempat Tidur, Fasilitas Kamar, dan Nurse Call Ruangan        │ [✓] Lengkap       │ Kamar 201-A Bersih/Siap │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│  PENGESAHAN TANDA TANGAN DIGITAL BERKAS SERAH TERIMA ADMISI:                                                                     │
│                                                                                                                                  │
│  ┌──────────────────────────────┐  ┌──────────────────────────────┐  ┌────────────────────────────────────────────────────────┐  │
│  │ 1. PETUGAS ADMISI PPRI       │  │ 2. PETUGAS CRO               │  │ 3. PERAWAT RUANGAN RAWAT INAP                          │  │
│  ├──────────────────────────────┤  ├──────────────────────────────┤  ├────────────────────────────────────────────────────────┤  │
│  │ [✓] TELAH DITANDATANGANI     │  │ [✓] TELAH DITANDATANGANI     │  │ [✓ TELAH DITANDATANGANI ] atau [ TANDA TANGANI SEKARANG ] │  │
│  │ Ns. Farhan Maulana, S.Kep    │  │ Maya Indriyani, S.Sos        │  │ Ns. Ratna Sari, S.Kep (PPJA Ruang Mawar)               │  │
│  │ 16 Okt 2026, 09:30 WIB       │  │ 16 Okt 2026, 09:40 WIB       │  │ Status Bed: OCCUPIED (Aktif Ditandatangani)             │  │
│  └──────────────────────────────┘  └──────────────────────────────┘  └────────────────────────────────────────────────────────┘  │
│  * Syarat Invariant: Tanda tangan perawat ruangan hanya aktif jika pasien telah menempati tempat tidur (Occupied).              │
└──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## Register Task

| Task ID / Status | Outcome | Requirement / Decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance Criteria | Verifikasi | Risiko / Pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-254` ✅ | Banner serah terima klinis aktif dan alert non-blocking kelengkapan berkas PPRI pada Detail Episode Rawat Inap | FR-TRF-002, FR-TRF-007 | 0.14.0 draft; lock TRF-DEL-20261016-01 | `InpatientEpisodeDetailView`, `TransferHandoverBanner`, `InformationAlert` | Pasang banner saat handover Sent; alert info kuning saat dokumen PPRI belum selesai | `BE-RWI-241` [BE], `BE-RWI-243` [BE] | TRF-AC-008, TRF-AC-009 | V-FE + UAT-TRF-01, 03 | UX perawat bangsal; konsistensi alert banner | D-FE |
| `FE-RWI-255` ✅ | Laci serah terima klinis (`TransferHandoverDrawer`) dengan area rekonsiliasi obat/alergi dan form pemilihan PPJA wajib | FR-TRF-003, FR-TRF-004, FR-TRF-005 | 0.14.0 draft | `TransferHandoverDrawer`, `BaseSelect`, `ConfirmModal` | Render badge alergi, tabel rekonsiliasi obat, dropdown PPJA wajib, disabled Terima Pasien bila PPJA kosong, dialog alasan tolak | `FE-RWI-254`, `BE-RWI-242` [BE] | TRF-AC-010, TRF-AC-011, TRF-AC-012 | V-FE + UAT-TRF-01, 02 | Validasi keselamatan pasien (PPJA); penanganan error 400/422 | D-FE |
| `FE-RWI-256` ✅ | Verifikasi dan penyelarasan aksi pratinjau & cetak Gelang Ranap Resmi dan Checklist Serah Terima Pasien Baru di Workspace PPRI | FR-TRF-006 | 0.14.0 draft | `AdmissionWorkspaceClient`, `WristbandAndLabelView`, `HandoverChecklistForm` | Verifikasi pratinjau cetak gelang ranap & label stiker; aktifkan tanda tangan perawat ruangan sesudah bed occupied | `FE-RWI-254` | TRF-AC-013, TRF-AC-014 | V-FE + UAT-TRF-01 | Kepatuhan SOP identifikasi pasien & serah terima berkas | D-FE |

---

## Batas Handoff dan Definition of Done

### D-FE — Bukti Wajib Setiap Task Frontend

Task frontend dikerjakan menggunakan konvensi Next.js App Router, JavaScript/JSX, Redux, Axios `InstanceAxios`, serta design token dan base component Quilvian (`@/components/features/base-features/`).

V-FE adalah bundel bukti frontend:
1. `npm run lint` lulus tanpa error baru.
2. `npm run build` lulus.
3. Review kesesuaian komponen visual dan responsivitas.
4. Laporan tracked disimpan di `task/report/frontend/<TASK-ID>.md`.

---

## Kartu Task

### FE-RWI-254 — Banner Serah Terima Klinis Aktif dan Alert Non-Blocking Berkas PPRI pada Detail Episode Rawat Inap

| Field | Isi |
| --- | --- |
| Status | Selesai (✅) |
| Outcome | Detail Episode Rawat Inap menampilkan banner serah terima aktif saat ada pasien transfer masuk dan alert info kuning yang menyatakan berkas PPRI sedang diproses tanpa menghalangi perawatan klinis |
| Ukuran / Batas | Integrasi komponen banner dan alert di `InpatientEpisodeDetailView.jsx` |
| Jejak | FR-TRF-002, FR-TRF-007; TRF-AC-008, TRF-AC-009 |
| Desain / Kontrak | Frontend 17.1; Kontrak Spesifikasi Transfer Bagian 5.A |
| Reuse / Disposisi | `InpatientEpisodeDetailView` (Extend), `TransferHandoverBanner` (Reuse), `InformationAlert` (Reuse) |
| Cakupan | Baca status handover dan `admissionDocumentsSummary` pada detail episode. Jika ada handover `Sent`, render `TransferHandoverBanner`. Jika `admissionDocumentsSummary.isAllCompleted == false`, render alert kuning info non-blocking. Hubungkan tombol banner ke state drawer |
| Batas Source | `src/components/view/health-services/inpatient-management/InpatientEpisodeDetailView.jsx`; `src/components/features/health-services/clinical-management/transfer-handover/TransferHandoverBanner.jsx` |
| Dependency Langsung | `BE-RWI-241` [BE], `BE-RWI-243` [BE] |
| Verifikasi yang Wajib | V-FE (`npm run lint`, `npm run build`) + uji tampilan banner & alert |
| Definition of Done | D-FE; seluruh AC terpetakan ke source; laporan `task/report/frontend/FE-RWI-254.md` |

**Acceptance Kriteria Positif**:
- Jika ada transfer handover berstatus `Sent`, banner amber tampil di bagian atas Detail Episode dengan tombol `[Lihat Detail Serah Terima]`.
- Jika dokumen admisi PPRI belum lengkap, alert kuning menampilkan: *"Dokumen admisi di Workspace PPRI sedang diproses ({CompletedCount}/{TotalCount} selesai). Pelayanan dan perawatan medis di bangsal tetap berjalan penuh."*
- Mengklik tombol banner membuka Laci Serah Terima Klinis (`TransferHandoverDrawer`).

**Acceptance Kriteria Negatif**:
- Jika tidak ada handover `Sent`, banner serah terima tidak tampil.
- Jika dokumen PPRI sudah 100% lengkap, alert kuning dokumen PPRI otomatis hilang.

---

### FE-RWI-255 — Laci Serah Terima Klinis dengan Area Rekonsiliasi Obat/Alergi dan Form Pemilihan PPJA Wajib

| Field | Isi |
| --- | --- |
| Status | Selesai (✅) |
| Outcome | Perawat bangsal dapat memverifikasi TTV, alat, riwayat alergi, merekonsiliasi obat dari unit asal, dan wajib memilih perawat PPJA sebelum menerima pasien |
| Ukuran / Batas | Pembaruan komponen `TransferHandoverDrawer.jsx` dan integrasi API accept/reject |
| Jejak | FR-TRF-003, FR-TRF-004, FR-TRF-005; TRF-AC-010, TRF-AC-011, TRF-AC-012 |
| Desain / Kontrak | Frontend 17.1; Kontrak Spesifikasi Transfer Bagian 5.A; Kamus Data 23.1, 23.2 |
| Reuse / Disposisi | `TransferHandoverDrawer` (Extend), `BaseSelect` (Reuse), `BaseButton` (Reuse) |
| Cakupan | Render badge riwayat alergi. Render tabel terapi obat IGD/Poli dengan pilihan Lanjut/Tunda/Stop. Render dropdown PPJA ruangan. Guard tombol `[Terima Pasien]` disabled saat PPJA kosong. Panggil `PATCH /transfer-handovers/{id}/accept` dengan payload lengkap. Tampilkan modal alasan saat klik `[Tolak / Minta Klarifikasi]` |
| Batas Source | `src/components/features/health-services/clinical-management/transfer-handover/TransferHandoverDrawer.jsx`; service/hook terkait |
| Dependency Langsung | `FE-RWI-254`, `BE-RWI-242` [BE] |
| Verifikasi yang Wajib | V-FE + uji fungsi form PPJA, rekonsiliasi obat, dan submit penerimaan |
| Definition of Done | D-FE; seluruh AC terpetakan ke source; laporan `task/report/frontend/FE-RWI-255.md` |

**Acceptance Kriteria Positif**:
- Perawat bangsal melihat rincian TTV, alat medis, dan tabel terapi obat dari unit asal.
- Memilih nama perawat PPJA mengaktifkan tombol `[Terima Pasien]`.
- Mengklik `[Terima Pasien]` mengirim request ke server, menutup drawer, dan menghilangkan banner serah terima.

**Acceptance Kriteria Negatif**:
- Tombol `[Terima Pasien]` berstatus disabled jika dropdown PPJA belum dipilih.
- Jika server merespons error `CLI-TRH-002` (akun login sama dengan pengirim), tampilkan toast/alert: *"Penerima serah terima klinis tidak boleh akun yang sama dengan perawat pengirim."*
- Mengklik tolak tanpa alasan min. 10 karakter menampilkan validasi form merah.

---

### FE-RWI-256 — Verifikasi dan Penyelarasan Aksi Pratinjau & Cetak Gelang Ranap Resmi dan Checklist Serah Terima Pasien Baru di Workspace PPRI

| Field | Isi |
| --- | --- |
| Status | Selesai (✅) |
| Outcome | Petugas PPRI dapat mencetak Gelang Ranap Resmi dan label medis untuk dipasangkan ke pasien, serta checklist serah terima berkas 15 butir dapat diselesaikan bertahap |
| Ukuran / Batas | Verifikasi dan penyelarasan tampilan di `AdmissionWorkspaceClient.jsx` |
| Jejak | FR-TRF-006; TRF-AC-013, TRF-AC-014 |
| Desain / Kontrak | Frontend 17.2; Kontrak Spesifikasi Transfer Bagian 5.B |
| Reuse / Disposisi | `AdmissionWorkspaceClient` (Reuse), `WristbandAndLabelView` (Reuse/Verify), `HandoverChecklistForm` (Reuse/Verify) |
| Cakupan | Pastikan pratinjau visual gelang memuat No RM, Nama, Bed, Kamar, dan Barcode. Pastikan tombol cetak gelang dan label stiker memicu print dialog. Pastikan checklist 15 butir serah terima admisi dapat ditandatangani perawat ruangan saat pasien sudah menempati bed |
| Batas Source | `src/components/view/health-services/inpatient-management/admission-workspace/sections/documents/wristband/`; `src/components/view/health-services/inpatient-management/admission-workspace/sections/documents/handover/` |
| Dependency Langsung | `FE-RWI-254` |
| Verifikasi yang Wajib | V-FE + verifikasi visual pratinjau dan fungsi print |
| Definition of Done | D-FE; seluruh AC terpetakan ke source; laporan `task/report/frontend/FE-RWI-256.md` |

**Acceptance Kriteria Positif**:
- Gelang ranap resmi memuat No RM, Nama Pasien, Tgl Lahir, Bed, dan Barcode/QR.
- Tombol cetak gelang memicu pencetakan thermal format gelang pasien.
- Formulir serah terima berkas 15 butir dapat ditandatangani secara elektronik oleh Admisi, CRO, dan Perawat Ruangan.

**Acceptance Kriteria Negatif**:
- Tanda tangan perawat ruangan pada checklist PPRI terkunci jika pasien belum berstatus menempati bed (`Occupied`).

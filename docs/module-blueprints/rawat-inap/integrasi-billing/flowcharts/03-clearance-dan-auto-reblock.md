# Flowchart Alur Clearance Kasir, Auto-Reblock, & Supervisor Override

Dokumen ini memodelkan proses persetujuan pemulangan kasir (*Billing Clearance*), penguncian otomatis kembali (*Auto-Reblock*) jika clearance dicabut, serta jalur darurat medis melalui *Supervisor Override*.

---

## 1. Diagram Alur Clearance & Auto-Reblock (Mermaid Flowchart)

```mermaid
flowchart TD
    subgraph Kasir_Keuangan["Staf Kasir & Penjamin"]
        K1["Dokter Izinkan Pasien Pulang"] --> K2["Review Seluruh Komponen Tagihan"]
        K2 --> K3["Terima Pembayaran / Konfirmasi Asuransi"]
        K3 --> K4["Setujui Clearance Kepulangan"]
        K7["Muncul Tagihan Susulan dari Farmasi/Lab"] --> K8["Kasir Cabut Persetujuan Clearance"]
    end

    subgraph Bangsal_Rawat_Inap["Staf Keperawatan & Supervisor Bangsal"]
        P1["Pantau Status di Layar Pasien"] --> P2{"Status Kasir Lunas (Cleared)?"}
        P2 -- "Belum Lunas" --> P3["Tombol Pulang Fisik Terkunci (Disabled)"]
        P2 -- "Sudah Disetujui" --> P4["Tombol Pulang Fisik Berubah Aktif"]
        P4 --> P5{"Apakah Muncul Sinyal Clearance Dicabut?"}
        P5 -- "Ya (Pencabutan Kasir)" --> P6["Sistem Kunci Otomatis (AUTO-REBLOCK)"]
        P6 --> P7{"Apakah Pasien Kritis / Rujukan Darurat?"}
        P7 -- "Tidak (Pasien Stabil)" --> P8["Tunggu Pelunasan Tagihan Susulan di Kasir"]
        P7 -- "Ya (Darurat Medis)" --> P9["Supervisor Buka Otorisasi Override"]
        P9 --> P10["Input Alasan Klinis & Verifikasi PIN"]
        P10 --> P11["Eksekusi Pelepasan Fisik Darurat"]
        P5 -- "Tidak (Aman)" --> P12["Perawat Konfirmasi Pasien Keluar Fisik"]
        P12 --> P13["Catat Jam Keluar Fisik & Lepas Kamar"]
        P11 --> P13
    end

    K4 -. "Sinyal Clearance Disetujui" .-> P2
    K8 -. "Sinyal Clearance Dicabut" .-> P5
```

---

## 2. Tabel Rincian Langkah Kerja

| No | Langkah Kerja | Pelaku / Aktor | Masukan (Input) | Keluaran (Output) | Tindakan Bila Mengalami Hambatan / Gagal |
|---:|---|---|---|---|---|
| 1 | Persetujuan Kasir | Staf Kasir | Pelunasan tagihan / penjaminan asuransi | Status clearance disetujui | Kasir wajib memeriksa apakah masih ada hasil lab/resep yang belum ditagihkan. |
| 2 | Pengaktifan Tombol Pulang | Sistem Rawat Inap | Sinyal clearance dari kasir | Tombol kepulangan fisik di layar perawat aktif | Bila sinyal terlambat masuk, perawat dapat menekan tombol periksa status kasir. |
| 3 | Pencabutan Clearance | Staf Kasir | Tagihan susulan yang baru diinput | Sinyal clearance dicabut terkirim ke bangsal | Kasir wajib segera menghubungi perawat bangsal via telepon internal. |
| 4 | Kunci Otomatis (Auto-Reblock) | Sistem Rawat Inap | Sinyal pembatalan clearance kasir | Tombol kepulangan fisik terkunci kembali seketika | Pasien dilarang meninggalkan ruangan; keluarga diminta menyelesaikan tagihan tambahan. |
| 5 | Evaluasi Kedaruratan Medis | Dokter / Supervisor Bangsal | Kondisi klinis memburuk / jadwal ambulans rujukan | Keputusan apakah pasien boleh tertahan atau harus segera jalan | Keselamatan nyawa pasien diutamakan di atas prosedur administrasi kasir. |
| 6 | Eksekusi Supervisor Override | Supervisor Bangsal | Formulir darurat, teks alasan medis, dan PIN otorisasi | Izin pemulangan fisik darurat disahkan | Alasan wajib memuat rincian kondisi gawat darurat dan rumah sakit rujukan. |
| 7 | Pelepasan Fisik Pasien | Perawat Bangsal | Pasien meninggalkan ruangan secara nyata | Jam keluar fisik terkunci; tempat tidur berstatus kosong | Perawat mencatat stempel waktu presisi agar perhitungan sewa kamar adil. |

# Farmasi — Frontend Roadmap

| Field | Nilai |
| --- | --- |
| Blueprint | `PHA-BP-001` revisi `4` · status `approved` |
| Bentuk blueprint | `SINGLE` |
| Frontend SHA | `1b138b9aac7a50524fd751a47c9a76e0a55f8803` |

Berkas ini lahir 21 September 2026 bersama slice Financial Clearance. Task frontend Routing Depo
(`PHA-FE-001`) tetap hidup pada `README.md` roadmap dan tidak dipindahkan ke sini.

---

# Gelombang Financial Clearance

| Field | Nilai |
| --- | --- |
| Masukan | `PHA-DEC-067`, `PHA-DEC-069`, `PHA-DEC-071` — `approved` 21 September 2026 |
| Contract version berlaku | `PHA-API-CLEARANCE-v1` — `approved` |

## Grafik Urutan Dependency

```text
[BE] PHA-BE-006 ─> PHA-FE-002
```

Legenda: `[BE]` adalah cermin baca-saja milik `backend-roadmap.md` modul ini. Task itu dihitung
dan dijadwalkan di roadmap backend, bukan di sini.

| Gelombang eksekusi | Task | Dapat berjalan paralel? |
| --- | --- | --- |
| 1 | `PHA-FE-002` | Tunggal pada gelombang ini |

Jumlah pasangan prasyarat→task: **satu**, sama persis dengan isi kolom `Dependency` di bawah.

## Task

### `PHA-FE-002` — Alasan penahanan terbaca pada layar kerja Farmasi

| Field | Isi |
| --- | --- |
| Outcome | Petugas yang sedang memegang obat mengetahui mengapa pekerjaannya terhenti, tanpa perlu mengklik apa pun dan tanpa menghubungi administrator untuk masalah yang ada di kasir |
| Jejak | `PHA-DEC-069`; skema tambahan pada `03-frontend-architecture.md` |
| Contract | `PHA-API-CLEARANCE-v1` — field tambahan pada response layar kerja resep |
| Kemampuan existing yang dipakai | Layar antrean dan layar kerja resep yang sudah ada beserta pola penanganan keadaannya |
| Cakupan yang diharapkan | Penanda keadaan finansial pada kolom keadaan; alasan penahanan yang terbaca langsung; penonaktifan tombol lanjut. **Nol** layar baru, **nol** butir menu baru, **nol** route baru |
| Dependency | `[BE] PHA-BE-006` |
| Acceptance criteria | Resep yang ditahan menampilkan alasannya tanpa petugas mengklik apa pun; tombol lanjut **dinonaktifkan dan tetap terlihat** — bukan disembunyikan; keadaan belum diketahui ditampilkan apa adanya, **tidak** sebagai siap dikerjakan; keadaan tertinggal ditampilkan sebagai tidak dapat dipastikan dan **tidak** menyediakan tombol coba paksa; daftar dimuat ulang setelah tindakan yang berhasil |
| Bukti verifikasi | Verifikasi terhadap kontrak `PHA-API-CLEARANCE-v1`; verifikasi keadaan memuat, kosong, gagal, belum diketahui, dan tertinggal; verifikasi bahwa tombol lanjut terkunci saat resep ditahan; bukti mengikuti kebijakan test frontend yang berlaku di repository ini |
| Risiko | **Menyembunyikan** tombol alih-alih menonaktifkannya akan membuat petugas mengira ia kehilangan kewenangan, lalu menghubungi administrator untuk masalah yang sebenarnya ada di kasir. Ini kesalahan yang paling mudah terjadi karena pola menyembunyikan tombol memang benar untuk kasus hak akses — tetapi ini bukan kasus hak akses |
| Pemilik | Frontend + Pharmacy |
| Definition of Done | Alasan penahanan terbaca langsung; tombol lanjut dinonaktifkan pada keadaan yang benar; nol layar dan nol butir menu baru; nol tombol override dalam bentuk apa pun |

## Kewenangan UI

| Hal | Kewenangan |
| --- | --- |
| Keberadaan penanda, keterbacaan alasan tanpa mengklik, tombol dinonaktifkan saat ditahan | **Terkunci** `03-frontend-architecture.md` — ketiganya menyangkut keselamatan dan kejelasan |
| Bunyi pesan bagi pengguna | Mengikuti `contracts/validation-matrix.md`, **MUST NOT** dikarang ulang di frontend |
| Ikon, warna penanda, posisi kolom, komponen | **`DEV_DISCRETION`** |

## Yang sengaja tidak dibuat

| Yang wajar diharapkan | Alasan |
| --- | --- |
| Tombol menandai resep sudah dibayar | Kewenangan itu dihapus permanen dari Farmasi oleh keputusan `1A` |
| Tombol memaksa lanjut saat sinkronisasi bermasalah | `PHA-DEC-067` — tidak ada override bagi peran mana pun |
| Tombol menyinkronkan ulang secara manual | Mengundang kebiasaan menekannya sampai hasilnya menyenangkan |
| Layar khusus keadaan finansial | Petugas butuh keterangan di tempat ia bekerja, bukan di layar terpisah |

## Traceability requirement ke bukti verifikasi

| Requirement | Task | Bukti verifikasi |
| --- | --- | --- |
| `FR-PHA-CLR-10` | `PHA-FE-002` | Verifikasi keadaan pada layar kerja; `PHA-AT-CLR-10` sebagai skenario pendamping sisi backend |

**Nol requirement frontend tanpa bukti verifikasi.**

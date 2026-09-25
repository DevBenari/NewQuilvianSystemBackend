# Bukti `LAB-EVD-011` — Keputusan pemilik modul atas `LAB-CONFLICT-014` dan pemakaian sebelum koreksi `S6`

| Field | Value |
|---|---|
| `evidence_id` | `LAB-EVD-011` |
| Menjawab | `LAB-CONFLICT-014` (`02-backend-architecture.md` 20.12) dan pertanyaan *"Bolehkah `S4` dipakai sebelum koreksi `S6` berdiri"* (`04-prd-to-mvp.md` 21.7, belum ber-Decision ID) |
| Penulis | Yoga Aji Pratama, pemilik modul |
| Diterima | Sesi 2026-09-25 malam, sebagai **dua tangkapan layar** berjudul *Keputusan* |
| Bentuk | Tangkapan layar teks. **Tangkapan kedua tampak terpotong** sesudah contoh JSON — bila ada butir lanjutan, ia belum tercatat di sini |
| Klarifikasi | Empat pertanyaan pilihan pada sesi yang sama — bagian C |
| Dicatat pada | `00-interview-decisions.md` — Amendment Pass Putaran 18 |

**Bagian A dan B disalin apa adanya dari tangkapan layar dan tidak boleh disunting.** Tafsirannya
ada pada decision log, bukan di sini.

---

## A. Tangkapan layar 1 — pemakaian sebelum koreksi `S6`

> **Keputusan**
>
> Sebelum koreksi S6 selesai:
>
> 1. Hasil pemeriksaan boleh digunakan hanya untuk:
>    - pemeriksaan internal laboratorium;
>    - proses review analis;
>    - verifikasi teknis.
> 2. Hasil tersebut **tidak boleh digunakan sebagai hasil resmi pasien** untuk:
>    - keputusan klinis dokter;
>    - cetak hasil final;
>    - pengiriman kepada pasien;
>    - integrasi eksternal.
> 3. Status hasil sebelum validasi:
>
>    ```
>    Draft
>      ↓
>    Pemeriksaan Selesai
>      ↓
>    Menunggu Validasi
>      ↓
>    Tervalidasi
>      ↓
>    Dirilis
>    ```
>
> 4. Hanya status:
>
>    ```
>    Tervalidasi + Dirilis
>    ```
>
> yang dianggap sebagai hasil resmi.

## B. Tangkapan layar 2 — `LAB-CONFLICT-014`

> **Keputusan**
>
> Status **Selesai** pada `LabOrder` tidak boleh ditentukan langsung oleh endpoint penyelesaian
> order.
>
> Sebelum status order berubah menjadi **Selesai**, sistem wajib memastikan:
>
> 1. Semua pemeriksaan dalam order:
>    - sudah memiliki hasil pemeriksaan;
>    - tidak berada pada status `Dalam Pemeriksaan`;
>    - tidak berada pada status `Menunggu Hasil`.
> 2. Pemeriksaan yang membutuhkan validasi klinis:
>    - sudah divalidasi oleh dokter yang memiliki kewenangan;
>    - memiliki catatan validator:
>      - nama;
>      - peran;
>      - tanggal dan waktu validasi.
> 3. Jika terdapat satu pemeriksaan saja yang belum memenuhi syarat:
>    - order tidak dapat menjadi `Selesai`;
>    - API mengembalikan `409 Conflict`;
>    - alasan kegagalan harus menjelaskan pemeriksaan mana yang belum memenuhi syarat.
>
> Contoh response:
>
> ```json
> {
>   "code": "LAB_ORDER_COMPLETION_BLOCKED",
>   "message": "Order belum dapat diselesaikan karena masih terdapat pemeriksaan yang belum tervalidasi.",
>   "details": [
>     {
>       "examinationId": "xxx",
>       "status": "Menunggu Validasi"
>     }
>   ]
> }
> ```

## C. Klarifikasi pada sesi yang sama

Kedua tangkapan layar dapat dibaca lebih dari satu cara pada empat butir. Masing-masing
ditanyakan sebagai pilihan tertutup; **jawaban pemilik modul dicatat apa adanya**.

| No | Pertanyaan | Pilihan yang dijawab | Bunyi pilihan itu |
|---:|---|---|---|
| 1 | Keputusan *pemakaian sebelum koreksi S6* dapat dibaca dua cara | **Rilis sudah resmi** | *"Sebelum S6 berdiri, validasi dan rilis boleh dipakai sungguhan: hasil Tervalidasi+Dirilis adalah hasil resmi pasien, yang belum dirilis hanya untuk internal lab. Penahan ini hilang dari MVP-9d/MVP-10c, tetapi prosedur bila hasil yang sudah dirilis ternyata keliru (sebelum S6 ada) masih perlu dijawab."* Pilihan yang tidak dipilih: *"Semua internal dulu"* — tidak ada hasil resmi sebelum `S6` dibangun |
| 2 | Pemetaan label keadaan hasil — tangkapan 1 dan 2 memakai kosakata berbeda | **Final = Menunggu Validasi** | *"Belum diisi = Menunggu Hasil; Draft = Draft; Final = Menunggu Validasi; lalu Tervalidasi dan Dirilis. 'Pemeriksaan Selesai' adalah nama tindakan analis (tombol Final), bukan keadaan tersendiri. 'Dalam Pemeriksaan' tetap label order. Nol perubahan model dan kontrak approved."* |
| 3 | Order boleh Selesai bila seluruh pemeriksaan tidak batal sudah… | **Dirilis** | *"Satu arti 'Selesai' dengan label order resultProgress dan AC-199. Syarat 'sudah divalidasi + nama, peran, waktu validator' di foto otomatis terpenuhi karena rilis mensyaratkan validasi. Detail 409 menyebut pemeriksaan yang belum dirilis beserta keadaannya."* |
| 4 | Pemeriksaan yang belum punya jalur validasi — Patologi Anatomi, Mikrobiologi sebelum `MVP-10`, hasil `Sementara` | **Tertahan** | *"Order itu tidak dapat Selesai sampai jalur validasinya ada; PUT complete menjawab 409 dengan rinciannya. Sejalan dengan 'hanya Tervalidasi + Dirilis yang resmi'. Hari ini nol layar memanggil PUT complete, jadi nol pengguna terhenti."* |

**Satu anggapan yang tidak ditanyakan dan dicatat terbuka:** pemeriksaan yang **batal atau gugur
tidak menahan** penyelesaian order. Tangkapan 2 tidak menyebutnya; `AC-199` yang sudah terkunci
menyatakannya.

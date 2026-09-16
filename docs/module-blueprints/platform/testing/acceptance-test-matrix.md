# Acceptance Test Matrix — Platform / Alokasi Nomor Bisnis

| Field | Nilai |
| --- | --- |
| Contract version | `v1` — ✅ **`approved`** `2026-09-09` |
| `last_changed_in` | `v1` |
| Traceability | `AC-PLT-001`..`AC-PLT-012` ← `DEC-PLT-002`, `DEC-PLT-004`, `DEC-PLT-008`, `INV-PLT-001`..`004` |

---

## 1. Kenapa sebagian besar wajib PostgreSQL

Yang diuji modul ini adalah **apa yang terjadi pada `COMMIT` dan `ROLLBACK`**, dan bagaimana dua
permintaan bersamaan diantrekan. Keduanya tidak ada pada provider InMemory: ia tidak punya
transaksi sungguhan dan tidak punya `pg_advisory_xact_lock`.

Menjalankan uji ini di InMemory akan **lulus tanpa membuktikan apa pun** — bentuk kegagalan yang
paling berbahaya, karena ia menghasilkan rasa aman yang keliru.

| Lapisan | Yang dibuktikan | Cukup? |
| --- | --- | --- |
| `UnitTests.InMemory` | Perakitan format, validasi parameter, pemilihan periode | Untuk itu saja |
| `UnitTests.Sqlite` | Bentuk tabel, index unik, check constraint | Untuk itu saja |
| **`IntegrationTests.Postgres`** | **Durabilitas dan antrean** | **Wajib** — nol pengganti |

---

## 2. Acceptance criteria

| ID | Skenario | Lapisan | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `AC-PLT-001` | Alokasi pertama pada deret yang belum pernah dipakai | Integ | Baris lahir, nilai `1`, nomor terakit sesuai awalan dan jumlah digit |
| `AC-PLT-002` | Alokasi kedua pada deret yang sama | Integ | Nilai `2`; nomor berbeda dari yang pertama |
| `AC-PLT-003` | **Pekerjaan pemanggil dibatalkan setelah nomor terbit** | **Integ** | **Pencacah tetap naik.** Alokasi berikutnya memberi nomor **berikutnya**, bukan mengulang yang hangus (`DEC-PLT-008`) |
| `AC-PLT-004` | Dua alokasi bersamaan pada deret yang sama | **Integ** | Dua nomor **berbeda**; nol kegagalan; nol nomor kembar (`INV-PLT-001`) |
| `AC-PLT-005` | Dua alokasi bersamaan pada deret **berbeda** | **Integ** | Keduanya selesai tanpa saling menunggu |
| `AC-PLT-006` | Deret `ResetPolicy = NEVER` melewati pergantian tahun | Integ | Pencacah **tidak** diulang; periode tetap `GLOBAL` (`DEC-PLT-004`, `INV-PLT-004`) |
| `AC-PLT-007` | Deret `ResetPolicy = DAILY` melewati pergantian hari | Unit + Integ | Periode berpindah; pencacah periode baru mulai dari `1`. Hanya untuk menampung deret lama |
| `AC-PLT-008` | Awalan kosong, terlalu panjang, atau jumlah digit di luar 4–12 | Unit | Ditolak `VAL-PLT-002`/`VAL-PLT-004`; **nol** nomor terbit |
| `AC-PLT-009` | Kebijakan pengulangan di luar empat nilai sah | Unit | Ditolak `VAL-PLT-003` |
| `AC-PLT-010` | Nilai pencacah melampaui jumlah digit yang dikonfigurasi | Unit | Ditolak `VAL-PLT-007`, **bukan** menerbitkan nomor yang bentuknya menyimpang |
| `AC-PLT-011` | Percobaan menyisipkan baris kembar `(SequenceKey, ScopeKey)` | Sqlite / Integ | Ditolak index unik |
| `AC-PLT-012` | Empat deret Billing dipanggil lewat jalurnya sendiri | Integ | Perilakunya **tidak berubah** — tetap dilayani mesin lama (`INV-PLT-003`, §`PLT-SLICE-02`) |

---

## 3. Skenario UAT

### Jalur berhasil

> Petugas Bank Darah membuat order darah pertama hari itu. Order tersimpan dengan nomor berakhiran
> `…0001`. Petugas membuat order kedua; nomornya `…0002`. Keduanya berbeda, berurutan, dan tidak
> pernah berubah setelahnya.

### Jalur gagal — dan inilah yang paling perlu dilihat pemilik

> Petugas membuat order darah. Sistem menerbitkan nomor `…0003`, lalu validasi menolak order itu
> karena unitnya tidak berwenang memesan darah. Order tidak tersimpan.
>
> Petugas memperbaiki dan menyimpan lagi. Order kali ini tersimpan dengan nomor **`…0004`**, bukan
> `…0003`.
>
> **Nomor `…0003` hilang selamanya dan tidak akan pernah muncul di mana pun.** Ini perilaku yang
> benar menurut `DEC-PLT-008`, bukan cacat — tetapi ia **akan terlihat** oleh petugas yang jeli,
> dan karena itu perlu diketahui pemilik proses sebelum rilis.

### Jalur gagal — alokasi bersamaan

> Dua petugas menyimpan order pada detik yang sama. Keduanya berhasil, dengan nomor berbeda dan
> berurutan. Tidak ada yang menerima galat, dan tidak ada nomor kembar.

---

## 4. Yang tidak diuji, beserta alasannya

| Tidak diuji | Alasan |
| --- | --- |
| Perpindahan empat deret Billing ke mesin baru | Lingkup `PLT-SLICE-02`; menuntut `DEC-PLT-006` dijawab |
| Perilaku deret hampir habis di luar penolakan `VAL-PLT-007` | `OQ-PLT-005` masih terbuka |
| Kode fasilitas di dalam awalan | `OQ-PLT-006` masih terbuka |
| Penelusuran nomor kembar yang mungkin sudah terlanjur terbit | `PLT-SLICE-04`; menuntut akses data produksi |
| Layar pemantauan | Belum ada wewenang UI; layar bukan bagian kemampuan inti |

---

## 5. Definition of Done pengujian

- [ ] `AC-PLT-003` dan `AC-PLT-004` lulus **di PostgreSQL sungguhan** — bukan InMemory
- [ ] Nol uji inti yang dinyatakan lulus berdasarkan provider yang tidak punya transaksi
- [ ] Empat deret Billing terbukti tidak berubah perilakunya (`AC-PLT-012`)
- [ ] Angka hasil uji dicatat apa adanya pada laporan task, termasuk yang `NOT RUN`

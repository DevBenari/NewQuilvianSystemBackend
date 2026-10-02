# Pengkajian Pasien dan Instrumen Klinis Berversi

| Field | Nilai |
| --- | --- |
| Sub-modul | `keperawatan` |
| Revision | `0.3` |
| Status | **`draft`** — belum disetujui manusia |
| Isi | Seluruh percabangan **beserta jalur pengecualiannya** |
| Kemampuan | `CAP-012` |
| Keputusan | `RWI-DEC-118` s.d. `120`, `124`, `131`, `136`, `141` |

---

## 1. Menyusun dan mengesahkan instrumen

```mermaid
flowchart TD
    subgraph ADM["Admin konfigurasi klinis"]
        A1["Buat versi baru dari versi terakhir"]
        A2["Ubah isian, pilihan, skor, kategori"]
        A3{"Kategori menutup semua skor tanpa celah?"}
        A4["Uji hitung dengan jawaban contoh"]
    end

    subgraph KOM["Komite keperawatan"]
        B1["Periksa isi dan hasil uji hitung"]
        B2{"Orang yang sama dengan pengubah terakhir?"}
        B3["Sahkan dengan catatan rapat"]
        B4["Versi lama berhenti berlaku"]
    end

    A1 --> A2 --> A3
    A3 -- Tidak --> A2
    A3 -- Ya --> A4 --> B1 --> B2
    B2 -- Ya --> X1["Ditolak — minta anggota komite lain"]
    B2 -- Tidak --> B3 --> B4
```

| No | Langkah | Pelaku | Masukan | Keluaran | Bila gagal, petugas melakukan |
| ---: | --- | --- | --- | --- | --- |
| 1 | Buat versi baru | Admin | Versi berlaku | Versi konsep | — |
| 2 | Ubah isi | Admin | Keputusan komite | Isi konsep | Isi pernah diubah orang lain → muat ulang lalu ulangi perubahan |
| 3 | Periksa kategori | Sistem | Batas kategori | Lolos atau ditolak | Betulkan batas; misal Rendah 0–24, Sedang 25–44, Tinggi 45 ke atas |
| 4 | Uji hitung | Admin | Jawaban contoh | Skor dan kategori | Hasil tidak sesuai harapan → betulkan skor pilihan |
| 5 | Sahkan | Anggota komite **selain** pengubah terakhir | Versi konsep | Versi berlaku | Pengubah terakhir mencoba mengesahkan → minta anggota lain |
| 6 | Versi lama berhenti | Sistem | Pengesahan | Satu versi berlaku | — |

**Contoh.** Andi mengubah Morse Dewasa menjadi v2 Senin 09.00. Andi tidak dapat mengesahkannya. Ns. Wati mengesahkan
Selasa 10.00; sejak itu pengkajian baru memakai v2, sedangkan hasil Senin tetap tercatat dengan v1.

---

## 2. Mengisi dokumen Pengkajian Pasien

```mermaid
flowchart TD
    subgraph PRW["Perawat unit"]
        P1["Pilih sub-menu Pengkajian Pasien"]
        P2{"Formulir berlaku untuk usia pasien tersedia?"}
        P3["Isi formulir"]
        P4["Simpan konsep"]
        P5{"Tekan Selesaikan"}
        P6["Dokumen selesai beserta skor dan versi"]
        P7["Salah setelah selesai: tambah addendum beralasan"]
    end

    subgraph SIS["Sistem"]
        S1{"Versi sudah disahkan atau lingkungan uji?"}
        S2{"Isian wajib lengkap?"}
        S3["Hitung skor dan kategori"]
        S4["Tampilkan alert bila kategori berisiko"]
        S5["Perbarui progres lima bagian"]
    end

    P1 --> P2
    P2 -- Tidak --> X1["Laporkan ke admin konfigurasi; jangan isi di kertas tanpa catatan"]
    P2 -- Ya --> P3 --> P4 --> S3 --> P5
    P5 --> S1
    S1 -- Tidak --> X2["Tetap konsep sampai versi disahkan"]
    S1 -- Ya --> S2
    S2 -- Tidak --> X3["Lengkapi isian yang disebut sistem"]
    X3 --> P3
    S2 -- Ya --> P6 --> S4 --> S5
    P6 -.-> P7
```

| No | Langkah | Pelaku | Masukan | Keluaran | Bila gagal, petugas melakukan |
| ---: | --- | --- | --- | --- | --- |
| 1 | Buka sub-menu | Perawat | Pasien terpilih | Riwayat dan formulir | Progres gagal dimuat → Coba Lagi; **jangan** menganggap bagian belum diisi |
| 2 | Formulir sesuai usia | Sistem | Tanggal lahir pasien | Formulir versi berlaku | Tidak ada instrumen untuk usia itu → laporkan ke admin |
| 3 | Isi formulir | Perawat | Hasil pengkajian | Isian di layar | Kajian Umum: pilih tanda vital yang sudah dicatat, jangan mengetik ulang angkanya |
| 4 | Simpan konsep | Perawat | Isian | Konsep dan skor sementara | Formulir berganti versi → muat ulang, isian di layar tetap ada |
| 5 | Selesaikan | Perawat | Konsep | Dokumen selesai | Versi belum sah → konsep tetap tersimpan; isian wajib kurang → lengkapi |
| 6 | Alert | Sistem | Kategori | Penanda di kepala pasien | Alert gagal dimuat → tampil sebagai galat, bukan hilang |
| 7 | Progres | Sistem | Keadaan lima dokumen | ✓ / ! / ○ | — |
| 8 | Koreksi setelah selesai | Penulis atau kepala ruangan | Alasan | Addendum bernomor | Isi asli tidak diubah |

**Monitoring Nyeri.** Sebelum skala, perawat memilih "tidak nyeri", "nyeri", atau "tidak dapat dinilai". Pasien tidak
sadar → "tidak dapat dinilai" dan instrumen perilaku bila versi berlaku menyediakannya; **tidak** dicatat "tidak nyeri".
Setelah intervensi, sistem menampilkan jam kajian ulang dari versi instrumen.

---

## 3. Evaluasi Awal MPP

```mermaid
flowchart TD
    subgraph MPP["MPP"]
        M1["Buka Evaluasi Awal pasien"]
        M2{"Pasien berada di unit penempatan saya?"}
        M3{"Episode sudah punya Evaluasi Awal?"}
        M4["Isi delapan bagian dan simpan konsep"]
        M5["Selesaikan"]
        M6["Tambah addendum bila perlu"]
    end

    subgraph PRW["Perawat unit"]
        P1["Membaca Evaluasi Awal tanpa mengubah"]
    end

    M1 --> M2
    M2 -- Tidak --> X1["Ditolak — MPP unit pasien yang menulis"]
    M2 -- Ya --> M3
    M3 -- Sudah selesai --> M6
    M3 -- Konsep milik MPP lain --> X2["Hubungi penulis konsep"]
    M3 -- Belum --> M4 --> M5 --> P1
```

| No | Langkah | Pelaku | Masukan | Keluaran | Bila gagal, petugas melakukan |
| ---: | --- | --- | --- | --- | --- |
| 1 | Periksa unit | Sistem | Unit episode saat simpan | Boleh atau tidak | Pasien baru dipindah → MPP unit baru |
| 2 | Periksa dokumen yang ada | Sistem | Episode | Buat baru atau lanjutkan | Sudah selesai → addendum, bukan dokumen kedua |
| 3 | Isi dan selesaikan | MPP | Checklist versi berlaku | Dokumen selesai | Checklist belum disahkan → tetap konsep |
| 4 | Addendum | MPP unit | Alasan | Addendum | Selama modul rekam medis belum menyediakan jenis dokumen ini, tombol nonaktif — catat kebutuhan koreksi kepada kepala ruangan |

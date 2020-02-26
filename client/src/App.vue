<template>
  <div id="app">
    <!-- <img src="./assets/logo.png"> -->
    <MainView/>
  </div>
</template>

<script>
import HelloWorld from './components/HelloWorld'
import Test from './components/Test'
import MainView from "./components/MainView"

export default {
  name: 'app',
  components: {
    HelloWorld,
    Test,
    MainView
  }
}
</script>

<style>
#app {
  font-family: 'Avenir', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 0px;
}
</style>
